package com.hanen.videodownloader

import android.content.ClipboardManager
import android.content.Context
import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Button
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.unit.dp
import com.hanen.videodownloader.ui.theme.VideoDownloaderTheme

class MainActivity : ComponentActivity() {

    private val viewModel: DownloadViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val initialUrl = extractSharedUrl(intent)

        setContent {
            VideoDownloaderTheme {
                Surface(modifier = Modifier.fillMaxSize()) {
                    DownloadScreen(
                        viewModel = viewModel,
                        initialUrl = initialUrl,
                        onPasteClicked = { readClipboardText() }
                    )
                }
            }
        }
    }

    override fun onNewIntent(intent: Intent) {
        super.onNewIntent(intent)
        setIntent(intent)
    }

    private fun extractSharedUrl(intent: Intent?): String {
        if (intent?.action == Intent.ACTION_SEND && intent.type == "text/plain") {
            return intent.getStringExtra(Intent.EXTRA_TEXT).orEmpty()
        }
        return ""
    }

    private fun readClipboardText(): String {
        val clipboard = getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
        val clip = clipboard.primaryClip ?: return ""
        if (clip.itemCount == 0) return ""
        return clip.getItemAt(0).coerceToText(this).toString()
    }
}

@Composable
fun DownloadScreen(
    viewModel: DownloadViewModel,
    initialUrl: String,
    onPasteClicked: () -> String
) {
    var url by remember { mutableStateOf(initialUrl) }
    val state by viewModel.uiState.collectAsState()
    val isBusy = state is DownloadUiState.Extracting || state is DownloadUiState.Downloading

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        Text(text = "영상 다운로더", style = MaterialTheme.typography.headlineSmall)
        Text(text = "Instagram / TikTok / X(Twitter) 공개 게시물 링크를 붙여넣으세요.")

        OutlinedTextField(
            value = url,
            onValueChange = { url = it },
            modifier = Modifier.fillMaxWidth(),
            label = { Text("게시물 링크") },
            singleLine = true,
            keyboardOptions = KeyboardOptions(imeAction = ImeAction.Done)
        )

        Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
            OutlinedButton(onClick = { url = onPasteClicked() }, enabled = !isBusy) {
                Text("붙여넣기")
            }
            Button(onClick = { viewModel.downloadVideo(url) }, enabled = !isBusy) {
                Text("다운로드")
            }
        }

        when (val current = state) {
            is DownloadUiState.Idle -> Unit
            is DownloadUiState.Extracting -> StatusRow("링크 분석 중...")
            is DownloadUiState.Downloading -> {
                StatusRow("다운로드 중... ${current.progress}%")
                LinearProgressIndicator(
                    progress = current.progress / 100f,
                    modifier = Modifier.fillMaxWidth()
                )
            }
            is DownloadUiState.Success ->
                StatusRow("저장 완료: ${current.fileName} (내 파일 > 동영상 > VideoDownloader)")
            is DownloadUiState.Error -> StatusRow("오류: ${current.message}")
        }
    }
}

@Composable
private fun StatusRow(text: String) {
    Text(text = text, style = MaterialTheme.typography.bodyMedium)
}
