import { useState, useEffect } from 'react'
import { Video, Zap, ZapOff } from 'lucide-react'

// TypeScript declaration cho Unity bridge
declare global {
  interface Window {
    Unity?: {
      call: (method: string, data: string) => void
    }
  }
}

export function CameraControls() {
  const [isRecording, setIsRecording] = useState(false)
  const [flashEnabled, setFlashEnabled] = useState(false)
  const [recordingTime, setRecordingTime] = useState(0)

  // Log khi component mount
  useEffect(() => {
    console.log('[CameraControls] Component mounted!')
  }, [])

  // Timer cho recording
  useEffect(() => {
    let interval: number | null = null
    
    if (isRecording) {
      interval = window.setInterval(() => {
        setRecordingTime(prev => prev + 1)
      }, 1000)
    } else {
      setRecordingTime(0)
    }
    
    return () => {
      if (interval) window.clearInterval(interval)
    }
  }, [isRecording])

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60)
    const secs = seconds % 60
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  }

  const handleCapture = () => {
    console.log('📸 Capture photo clicked')
  }

  const handleRecordToggle = () => {
    const newState = !isRecording
    setIsRecording(newState)
    console.log(newState ? '🎥 Start recording clicked' : '⏹️ Stop recording clicked')
  }

  const handleFlashToggle = () => {
    const newState = !flashEnabled
    setFlashEnabled(newState)
    console.log(newState ? '💡 Flash ON clicked' : '💡 Flash OFF clicked')
  }

  return (
    <div className="fixed bottom-0 left-0 right-0 pb-8 px-6">
      {/* Recording timer */}
      {isRecording && (
        <div className="absolute top-0 left-1/2 -translate-x-1/2 -translate-y-16">
          <div className="bg-red-500 text-white px-4 py-2 rounded-full font-mono text-lg font-bold flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-white animate-pulse" />
            {formatTime(recordingTime)}
          </div>
        </div>
      )}

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
          disabled={isRecording}
          className="w-20 h-20 rounded-full bg-white border-4 border-white shadow-lg active:scale-95 transition-transform flex items-center justify-center disabled:opacity-50"
        >
          <div className="w-16 h-16 rounded-full bg-white border-2 border-gray-300" />
        </button>

        {/* Record button */}
        <button
          onClick={handleRecordToggle}
          className="w-14 h-14 rounded-full bg-black/30 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform"
        >
          {isRecording ? (
            <div className="w-6 h-6 rounded bg-red-500" />
          ) : (
            <Video className="w-6 h-6 text-white" />
          )}
        </button>
      </div>
    </div>
  )
}
