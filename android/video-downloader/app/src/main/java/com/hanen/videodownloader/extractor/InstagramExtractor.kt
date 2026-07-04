package com.hanen.videodownloader.extractor

import com.hanen.videodownloader.network.HttpClientProvider
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.Request
import org.jsoup.Jsoup

/**
 * Reads the public "embed" page for a post/reel, which exposes an
 * `<meta property="og:video">` tag with a direct MP4 URL for public posts.
 * Private accounts and login-only content (stories, etc.) are out of scope.
 */
class InstagramExtractor : VideoExtractor {

    override val platform = Platform.INSTAGRAM

    private val urlPattern = Regex("""instagram\.com/(?:reel|p|tv)/([A-Za-z0-9_-]+)""")

    override fun matches(url: String): Boolean = urlPattern.containsMatchIn(url)

    override suspend fun extract(url: String): ExtractedVideo = withContext(Dispatchers.IO) {
        val shortcode = urlPattern.find(url)?.groupValues?.get(1)
            ?: throw UnsupportedUrlException("Instagram 게시물 링크가 아니에요.")

        val request = Request.Builder()
            .url("https://www.instagram.com/p/$shortcode/embed/captioned/")
            .header("User-Agent", HttpClientProvider.DESKTOP_USER_AGENT)
            .build()

        HttpClientProvider.client.newCall(request).execute().use { response ->
            if (!response.isSuccessful) {
                throw VideoExtractionException("Instagram 페이지를 불러오지 못했어요 (HTTP ${response.code}).")
            }
            val html = response.body?.string()
                ?: throw VideoExtractionException("Instagram 응답이 비어 있어요.")

            val videoUrl = extractOgVideo(html)
                ?: throw VideoExtractionException(
                    "동영상을 찾지 못했어요. 비공개 계정이거나 동영상이 없는 게시물일 수 있어요."
                )

            ExtractedVideo(
                directUrl = videoUrl,
                suggestedFileName = "instagram_$shortcode.mp4",
                platform = platform
            )
        }
    }

    private fun extractOgVideo(html: String): String? {
        val og = Jsoup.parse(html).selectFirst("meta[property=og:video]")?.attr("content")
        if (!og.isNullOrBlank()) return og.replace("&amp;", "&")

        val match = Regex(""""video_url":"([^"]+)"""").find(html)
        return match?.groupValues?.get(1)?.replace("\\u0026", "&")?.replace("\\/", "/")
    }
}
