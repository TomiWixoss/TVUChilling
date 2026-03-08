import { useState } from 'react'
import { ModeSelector } from '@/components/ModeSelector'
import { CameraControls } from '@/components/CameraControls'
import { PlacementControls } from '@/components/PlacementControls'
import { Toaster } from '@/components/ui/sonner'
import { ArrowLeft } from 'lucide-react'

type ARMode = 'none' | 'imageTracking' | 'placement'

export function App() {
  const [currentMode, setCurrentMode] = useState<ARMode>('none')

  const handleModeSelect = (mode: 'imageTracking' | 'placement') => {
    setCurrentMode(mode)
  }

  const handleBackToMenu = () => {
    console.log('[App] Back to menu')
    setCurrentMode('none')
    
    // Reset AR mode trong Unity
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onModeSelect', 
        data: 'none' 
      })
      window.Unity.call(message)
    }
  }

  return (
    <>
      {currentMode === 'none' ? (
        // Mode selection screen
        <ModeSelector onModeSelect={handleModeSelect} />
      ) : (
        // AR camera view
        <>
          {/* Placement controls - Render trước để không che back button */}
          {currentMode === 'placement' && <PlacementControls />}

          {/* Back button - z-index cao hơn */}
          <button
            onClick={handleBackToMenu}
            className="fixed top-6 left-6 w-12 h-12 rounded-full bg-black/30 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform z-50"
          >
            <ArrowLeft className="w-6 h-6 text-white" />
          </button>

          {/* Camera controls - Luôn hiện */}
          <CameraControls />
        </>
      )}

      {/* Toast notifications */}
      <Toaster />
    </>
  )
}

export default App
