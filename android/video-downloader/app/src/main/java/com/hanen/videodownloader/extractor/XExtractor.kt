package com.hanen.videodownloader.extractor

import com.hanen.videodownloader.network.HttpClientProvider
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.Request
import org.json.JSONObject

/**
 * Uses the open-source vxtwitter.com mirror, which re-serves a public tweet's
 * metadata (including direct video URLs) as JSON without requiring auth.
 * If that mirror ever goes offline, this extractor breaks until swapped out.
 */
class XExtractor : VideoExtractor {

    override val platform = Platform.X

    private val urlPattern = Regex("""(?:twitter|x)\.com/([^/?]+)/status/(\d+)""")

    override fun matches(url: String): Boolean = urlPattern.containsMatchIn(url)

    override suspend fun extract(url: String): ExtractedVideo = withContext(Dispatchers.IO) {
        val match = urlPattern.find(url)
            ?: throw UnsupportedUrlException("X(Twitter) 게시물 링크가 아니에요.")
        val username = match.groupValues[1]
        val tweetId = match.groupValues[2]

        val request = Request.Builder()
            .url("https://api.vxtwitter.com/$username/status/$tweetId")
            .header("User-Agent", HttpClientProvider.DESKTOP_USER_AGENT)
            .build()

        HttpClientProvider.client.newCall(request).execute().use { response ->
            if (!response.isSuccessful) {
                throw VideoExtractionException(
                    "게시물을 불러오지 못했어요 (HTTP ${response.code}). 비공개 계정일 수 있어요."
                )
            }
            val body = response.body?.string()
                ?: throw VideoExtractionException("응답이 비어 있어요.")

            val mediaUrls = JSONObject(body).optJSONArray("mediaURLs")
            val videoUrl = (0 until (mediaUrls?.length() ?: 0))
                .map { mediaUrls!!.getString(it) }
                .firstOrNull { it.endsWith(".mp4") }
                ?: throw VideoExtractionException("이 게시물에는 동영상이 없어요.")

            ExtractedVideo(
                directUrl = videoUrl,
                suggestedFileName = "x_$tweetId.mp4",
                platform = platform
            )
        }
    }
}
