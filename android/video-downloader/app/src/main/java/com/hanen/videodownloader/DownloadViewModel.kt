package com.hanen.videodownloader

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.hanen.videodownloader.download.VideoDownloader
import com.hanen.videodownloader.extractor.ExtractorRegistry
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

sealed class DownloadUiState {
    data object Idle : DownloadUiState()
    data object Extracting : DownloadUiState()
    data class Downloading(val progress: Int) : DownloadUiState()
    data class Success(val fileName: String) : DownloadUiState()
    data class Error(val message: String) : DownloadUiState()
}

class DownloadViewModel(application: Application) : AndroidViewModel(application) {

    private val _uiState = MutableStateFlow<DownloadUiState>(DownloadUiState.Idle)
    val uiState: StateFlow<DownloadUiState> = _uiState.asStateFlow()

    fun downloadVideo(url: String) {
        if (_uiState.value is DownloadUiState.Extracting || _uiState.value is DownloadUiState.Downloading) return

        viewModelScope.launch {
            _uiState.value = DownloadUiState.Extracting
            try {
                val extracted = ExtractorRegistry.extract(url)
                _uiState.value = DownloadUiState.Downloading(0)
                VideoDownloader.download(getApplication(), extracted) { progress ->
                    _uiState.value = DownloadUiState.Downloading(progress)
                }
                _uiState.value = DownloadUiState.Success(extracted.suggestedFileName)
            } catch (e: Exception) {
                _uiState.value = DownloadUiState.Error(e.message ?: "알 수 없는 오류가 발생했어요.")
            }
        }
    }
}
