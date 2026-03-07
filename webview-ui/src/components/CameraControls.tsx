import { useState } from 'react'
import { Video, Zap, ZapOff } from 'lucide-react'

export function CameraControls() {
  const [isRecording, setIsRecording] = useState(false)
  const [flashEnabled, setFlashEnabled] = useState(false)

  const handleCapture = () => {
    console.log('📸 Capture photo')
    if (window.Unity) {
      window.Unity.call('onCapturePhoto', '')
    }
  }

  const handleRecordToggle = () => {
    const newState = !isRecording
    setIsRecording(newState)
    console.log(newState ? '🎥 Start recording' : '⏹️ Stop recording')
    
    if (window.Unity) {
      window.Unity.call('onRecordToggle', newState ? 'start' : 'stop')
    }
  }

  const handleFlashToggle = () => {
    const newState = !flashEnabled
    setFlashEnabled(newState)
    console.log(newState ? '💡 Flash ON' : '💡 Flash OFF')
    
    if (window.Unity) {
      window.Unity.call('onFlashToggle', newState ? 'on' : 'off')
    }
  }

  return (
    <div className="fixed bottom-0 left-0 right-0 pb-8 px-6">
      <div className="flex items-center justify-between max-w-md mx-auto">
        {/* Flash button */}
        <button
          onClick={handleFlashToggle}
          className="w-14 h-14 rounded-full bg-black/30 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform"
        >
          {flashEnabled ? (
            <Zap className="w-6 h-6 text-yellow-400 fill-yellow-400" />
          ) : (
            <ZapOff className="w-6 h-6 text-white" />
          )}
        </button>

        {/* Capture button */}
        <button
          onClick={handleCapture}
          className="w-20 h-20 rounded-full bg-white border-4 border-white shadow-lg active:scale-95 transition-transform flex items-center justify-center"
        >
          <div className="w-16 h-16 rounded-full bg-white border-2 border-gray-300" />
        </button>

        {/* Record button */}
        <button
          onClick={handleRecordToggle}
          className="w-14 h-14 rounded-full bg-black/30 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform"
        >
          {isRecording ? (
            <div className="w-6 h-6 rounded bg-red-500 animate-pulse" />
          ) : (
            <Video className="w-6 h-6 text-white" />
          )}
        </button>
      </div>
    </div>
  )
}
