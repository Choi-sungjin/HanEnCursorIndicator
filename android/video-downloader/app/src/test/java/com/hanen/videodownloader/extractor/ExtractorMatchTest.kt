package com.hanen.videodownloader.extractor

import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class ExtractorMatchTest {

    private val instagram = InstagramExtractor()
    private val tiktok = TikTokExtractor()
    private val x = XExtractor()

    @Test
    fun instagram_matchesReelAndPostLinks() {
        assertTrue(instagram.matches("https://www.instagram.com/reel/Cabc123XYZ/"))
        assertTrue(instagram.matches("https://www.instagram.com/p/Cabc123XYZ/?utm_source=ig"))
        assertFalse(instagram.matches("https://www.tiktok.com/@user/video/123"))
    }

    @Test
    fun tiktok_matchesShareAndCanonicalLinks() {
        assertTrue(tiktok.matches("https://www.tiktok.com/@someuser/video/7123456789012345678"))
        assertTrue(tiktok.matches("https://vm.tiktok.com/ZMabcDEfg/"))
        assertFalse(tiktok.matches("https://x.com/someuser/status/123"))
    }

    @Test
    fun x_matchesTwitterAndXStatusLinks() {
        assertTrue(x.matches("https://x.com/someuser/status/1234567890123456789"))
        assertTrue(x.matches("https://twitter.com/someuser/status/1234567890123456789"))
        assertFalse(x.matches("https://www.instagram.com/p/Cabc123XYZ/"))
    }
}
