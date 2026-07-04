package com.hanen.videodownloader.download

import android.content.ContentValues
import android.content.Context
import android.net.Uri
import android.provider.MediaStore
import com.hanen.videodownloader.extractor.ExtractedVideo
import com.hanen.videodownloader.network.HttpClientProvider
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.Request
import java.io.IOException

/** Streams the resolved direct video URL straight into MediaStore (Movies/VideoDownloader). */
object VideoDownloader {

    private const val BUFFER_SIZE = 8 * 1024

    suspend fun download(
        context: Context,
        video: ExtractedVideo,
        onProgress: (Int) -> Unit
    ): Uri = withContext(Dispatchers.IO) {
        val request = Request.Builder()
            .url(video.directUrl)
            .header("User-Agent", HttpClientProvider.DESKTOP_USER_AGENT)
            .build()

        HttpClientProvider.client.newCall(request).execute().use { response ->
            if (!response.isSuccessful) {
                throw IOException("동영상을 내려받지 못했어요 (HTTP ${response.code}).")
            }
            val body = response.body ?: throw IOException("응답 본문이 비어 있어요.")
            val contentLength = body.contentLength()

            val resolver = context.contentResolver
            val values = ContentValues().apply {
                put(MediaStore.Video.Media.DISPLAY_NAME, video.suggestedFileName)
                put(MediaStore.Video.Media.MIME_TYPE, "video/mp4")
                put(MediaStore.Video.Media.RELATIVE_PATH, "Movies/VideoDownloader")
                put(MediaStore.Video.Media.IS_PENDING, 1)
            }

            val collection = MediaStore.Video.Media.getContentUri(MediaStore.VOLUME_EXTERNAL_PRIMARY)
            val itemUri = resolver.insert(collection, values)
                ?: throw IOException("저장 위치를 만들지 못했어요.")

            val outputStream = resolver.openOutputStream(itemUri)
                ?: throw IOException("저장 스트림을 열지 못했어요.")

            outputStream.use { output ->
                body.byteStream().use { input ->
                    val buffer = ByteArray(BUFFER_SIZE)
                    var bytesCopied = 0L
                    var read = input.read(buffer)
                    while (read >= 0) {
                        output.write(buffer, 0, read)
                        bytesCopied += read
                        if (contentLength > 0) {
                            onProgress(((bytesCopied * 100) / contentLength).toInt())
                        }
                        read = input.read(buffer)
                    }
                }
            }

            values.clear()
            values.put(MediaStore.Video.Media.IS_PENDING, 0)
            resolver.update(itemUri, values, null, null)

            itemUri
        }
    }
}
