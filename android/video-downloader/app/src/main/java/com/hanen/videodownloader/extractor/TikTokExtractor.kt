package com.hanen.videodownloader.extractor

import com.hanen.videodownloader.network.HttpClientProvider
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.Request
import org.json.JSONObject
import java.net.URLEncoder

/**
 * Two-step lookup: first parse the embedded `__UNIVERSAL_DATA_FOR_REHYDRATION__`
 * JSON blob on the TikTok watch page for a play address, and fall back to the
 * community tikwm.com mirror API if TikTok's page structure has changed.
 */
class TikTokExtractor : VideoExtractor {

    override val platform = Platform.TIKTOK

    private val hostPattern = Regex("""tiktok\.com""")
    private val videoIdPattern = Regex("""/video/(\d+)""")

    override fun matches(url: String): Boolean = hostPattern.containsMatchIn(url)

    override suspend fun extract(url: String): ExtractedVideo = withContext(Dispatchers.IO) {
        val resolvedUrl = resolveRedirect(url)
        val videoId = videoIdPattern.find(resolvedUrl)?.groupValues?.get(1)

        extractFromPage(resolvedUrl, videoId)
            ?: extractFromMirrorApi(resolvedUrl, videoId)
            ?: throw VideoExtractionException(
                "동영상을 찾지 못했어요. TikTok 페이지 구조가 바뀌었거나 비공개 영상일 수 있어요."
            )
    }

    private fun resolveRedirect(url: String): String {
        val request = Request.Builder()
            .url(url)
            .head()
            .header("User-Agent", HttpClientProvider.DESKTOP_USER_AGENT)
            .build()
        return runCatching {
            HttpClientProvider.client.newCall(request).execute().use { it.request.url.toString() }
        }.getOrDefault(url)
    }

    private fun extractFromPage(url: String, videoId: String?): ExtractedVideo? {
        val request = Request.Builder()
            .url(url)
            .header("User-Agent", HttpClientProvider.DESKTOP_USER_AGENT)
            .build()

        return runCatching {
            HttpClientProvider.client.newCall(request).execute().use { response ->
                if (!response.isSuccessful) return@use null
                val html = response.body?.string() ?: return@use null

                val dataMatch = Regex(
                    """<script id="__UNIVERSAL_DATA_FOR_REHYDRATION__"[^>]*>(.*?)</script>""",
                    RegexOption.DOT_MATCHES_ALL
                ).find(html) ?: return@use null

                val videoUrl = findPlayAddr(dataMatch.groupValues[1]) ?: return@use null

                ExtractedVideo(
                    directUrl = videoUrl.replace("\\u0026", "&"),
                    suggestedFileName = "tiktok_${videoId ?: System.currentTimeMillis()}.mp4",
                    platform = platform
                )
            }
        }.getOrNull()
    }

    private fun findPlayAddr(json: String): String? {
        val match = Regex(""""playAddr":"([^"]+)"""").find(json)
            ?: Regex(""""downloadAddr":"([^"]+)"""").find(json)
        return match?.groupValues?.get(1)
    }

    private fun extractFromMirrorApi(url: String, videoId: String?): ExtractedVideo? {
        val encodedUrl = URLEncoder.encode(url, "UTF-8")
        val request = Request.Builder()
            .url("https://www.tikwm.com/api/?url=$encodedUrl")
            .header("User-Agent", HttpClientProvider.DESKTOP_USER_AGENT)
            .build()

        return runCatching {
            HttpClientProvider.client.newCall(request).execute().use { response ->
                if (!response.isSuccessful) return@use null
                val body = response.body?.string() ?: return@use null
                val json = JSONObject(body)
                if (json.optInt("code") != 0) return@use null
                val data = json.optJSONObject("data") ?: return@use null
                val playUrl = data.optString("play").takeIf { it.isNotBlank() } ?: return@use null
                val resolvedPlayUrl = if (playUrl.startsWith("http")) playUrl else "https://www.tikwm.com$playUrl"

                ExtractedVideo(
                    directUrl = resolvedPlayUrl,
                    suggestedFileName = "tiktok_${videoId ?: System.currentTimeMillis()}.mp4",
                    platform = platform
                )
            }
        }.getOrNull()
    }
}
