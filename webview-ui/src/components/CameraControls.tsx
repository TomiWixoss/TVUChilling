import { useState, useEffect } from 'react'
import { Camera, Video, Image as ImageIcon } from 'lucide-react'

// TypeScript declaration cho Unity bridge
declare global {
  interface Window {
    Unity?: {
      call: (message: string) => void
    }
  }
}

type CaptureMode = 'photo' | 'video'

interface CameraControlsProps {
  visible: boolean
}

export function CameraControls({ visible }: CameraControlsProps) {
  const [isRecording, setIsRecording] = useState(false)
  const [captureMode, setCaptureMode] = useState<CaptureMode>('photo')
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
    if (captureMode === 'photo') {
      console.log('📸 Capture photo clicked')
      if (window.Unity && window.Unity.call) {
        const message = JSON.stringify({ method: 'onCapturePhoto', data: '' })
        window.Unity.call(message)
      }
    } else {
      handleRecordToggle()
    }
  }

  const handleRecordToggle = () => {
    const newState = !isRecording
    setIsRecording(newState)
    console.log(newState ? '🎥 Start recording' : '⏹️ Stop recording')
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onRecordToggle', 
        data: newState ? 'start' : 'stop' 
      })
      window.Unity.call(message)
    }
  }

  const handleModeToggle = () => {
    const newMode = captureMode === 'photo' ? 'video' : 'photo'
    setCaptureMode(newMode)
    console.log(`📷 Mode switched to: ${newMode}`)
  }

  const handleGalleryOpen = () => {
    console.log('🖼️ Open gallery')
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ method: 'onGalleryOpen', data: '' })
      window.Unity.call(message)
    }
  }

  if (!visible) return null

  return (
    <div className="fixed bottom-0 left-0 right-0 pb-safe z-30">
      {/* Black background */}
      <div className="bg-black pt-6 pb-8 px-6">
        {/* Recording timer */}
        {isRecording && (
          <div className="absolute top-0 left-1/2 -translate-x-1/2 -translate-y-12">
            <div className="bg-red-500 text-white px-4 py-2 rounded-full font-mono text-lg font-bold flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-white animate-pulse" />
              {formatTime(recordingTime)}
            </div>
          </div>
        )}

        <div className="flex items-center justify-between max-w-md mx-auto">
          {/* Gallery button - Left */}
          <button
            onClick={handleGalleryOpen}
            disabled={isRecording}
            className="w-14 h-14 rounded-xl bg-white/10 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform disabled:opacity-50"
          >
            <ImageIcon className="w-6 h-6 text-white" />
          </button>

          {/* Capture button - Center */}
          <button
            onClick={handleCapture}
            className={`w-20 h-20 rounded-full border-4 shadow-lg active:scale-95 transition-all flex items-center justify-center ${
              captureMode === 'photo'
                ? 'bg-white border-white'
                : isRecording
                ? 'bg-red-500 border-red-500'
                : 'bg-white border-white'
            }`}
          >
            {captureMode === 'video' && isRecording ? (
              <div className="w-6 h-6 rounded bg-white" />
            ) : (
              <div className={`w-16 h-16 rounded-full ${
                captureMode === 'photo' ? 'bg-white border-2 border-gray-300' : 'bg-red-500'
              }`} />
            )}
          </button>

          {/* Mode toggle - Right */}
          <button
            onClick={handleModeToggle}
            disabled={isRecording}
            className="w-14 h-14 rounded-xl bg-white/10 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform disabled:opacity-50"
          >
            {captureMode === 'photo' ? (
              <Video className="w-6 h-6 text-white" />
            ) : (
              <Camera className="w-6 h-6 text-white" />
            )}
          </button>
        </div>
      </div>
    </div>
  )
}
