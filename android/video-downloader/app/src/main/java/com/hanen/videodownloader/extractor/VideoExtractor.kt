package com.hanen.videodownloader.extractor

/** A direct, playable video URL resolved from a social post link. */
data class ExtractedVideo(
    val directUrl: String,
    val suggestedFileName: String,
    val platform: Platform
)

enum class Platform(val displayName: String) {
    INSTAGRAM("Instagram"),
    TIKTOK("TikTok"),
    X("X (Twitter)")
}

interface VideoExtractor {
    val platform: Platform
    fun matches(url: String): Boolean
    suspend fun extract(url: String): ExtractedVideo
}

class UnsupportedUrlException(message: String) : Exception(message)
class VideoExtractionException(message: String, cause: Throwable? = null) : Exception(message, cause)

/**
 * Picks the right extractor for a pasted link. Only public posts are supported -
 * there is no login flow, so private accounts / protected posts will fail here.
 */
object ExtractorRegistry {

    private val extractors: List<VideoExtractor> = listOf(
        InstagramExtractor(),
        TikTokExtractor(),
        XExtractor()
    )

    suspend fun extract(rawUrl: String): ExtractedVideo {
        val url = rawUrl.trim()
        if (url.isEmpty()) {
            throw UnsupportedUrlException("링크를 입력해 주세요.")
        }
        val extractor = extractors.firstOrNull { it.matches(url) }
            ?: throw UnsupportedUrlException(
                "지원하지 않는 링크예요. Instagram, TikTok, X(Twitter) 링크만 지원합니다."
            )
        return extractor.extract(url)
    }
}
