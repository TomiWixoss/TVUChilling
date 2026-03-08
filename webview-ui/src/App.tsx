import { useState, useEffect } from 'react'
import { TopBar } from '@/components/TopBar'
import { ModelSelector } from '@/components/ModelSelector'
import { EditPanel } from '@/components/EditPanel'
import { CameraControls } from '@/components/CameraControls'
import { Toaster } from '@/components/ui/sonner'
import { AnimatePresence } from 'framer-motion'

// TypeScript declaration cho Unity bridge
declare global {
  interface Window {
    Unity?: {
      call: (message: string) => void
    }
    receiveUnityMessage?: (message: string) => void
  }
}

type UIMode = 'place' | 'edit'

export function App() {
  const [flashEnabled, setFlashEnabled] = useState(false)
  const [trackingEnabled, setTrackingEnabled] = useState(false)
  const [modelSelectorExpanded, setModelSelectorExpanded] = useState(false)
  const [selectedModel, setSelectedModel] = useState<string | null>('none')
  const [uiMode, setUIMode] = useState<UIMode>('place')
  const [selectedObjectName, setSelectedObjectName] = useState<string>()

  useEffect(() => {
    console.log('[App] Mounted - AR View Only')
    
    // Listen for Unity messages
    window.receiveUnityMessage = (message: string) => {
      try {
        const data = JSON.parse(message)
        console.log('[App] Received from Unity:', data)
        
        if (data.method === 'onObjectSelected') {
          setUIMode('edit')
          setSelectedObjectName(data.data)
          setModelSelectorExpanded(false)
        } else if (data.method === 'onObjectDeselected') {
          setUIMode('place')
          setSelectedObjectName(undefined)
        }
      } catch (e) {
        console.error('[App] Failed to parse Unity message:', e)
      }
    }

    return () => {
      window.receiveUnityMessage = undefined
    }
  }, [])

  const handleFlashToggle = () => {
    const newState = !flashEnabled
    setFlashEnabled(newState)
    console.log(newState ? '💡 Flash ON' : '💡 Flash OFF')
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onFlashToggle', 
        data: newState ? 'on' : 'off' 
      })
      window.Unity.call(message)
    }
  }

  const handleTrackingToggle = () => {
    const newState = !trackingEnabled
    setTrackingEnabled(newState)
    console.log(newState ? '🎯 Tracking ON' : '🎯 Tracking OFF')
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onTrackingToggle', 
        data: newState ? 'on' : 'off' 
      })
      window.Unity.call(message)
    }
  }

  const handleModelSelect = (modelId: string) => {
    setSelectedModel(modelId)
    console.log(`[App] Selected model: ${modelId}`)
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onModelSelect', 
        data: modelId 
      })
      window.Unity.call(message)
    }
  }

  const handleDelete = () => {
    console.log('[App] Delete selected object')
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onDeleteObject', 
        data: '' 
      })
      window.Unity.call(message)
    }
    
    setUIMode('place')
    setSelectedObjectName(undefined)
  }

  const handleDuplicate = () => {
    console.log('[App] Duplicate selected object')
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onDuplicateObject', 
        data: '' 
      })
      window.Unity.call(message)
    }
  }

  const handleDeselect = () => {
    console.log('[App] Deselect object')
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onDeselectObject', 
        data: '' 
      })
      window.Unity.call(message)
    }
    
    setUIMode('place')
    setSelectedObjectName(undefined)
  }

  return (
    <>
      {/* Top Bar */}
      <TopBar
        flashEnabled={flashEnabled}
        trackingEnabled={trackingEnabled}
        onFlashToggle={handleFlashToggle}
        onTrackingToggle={handleTrackingToggle}
      />

      {/* Bottom UI - Conditional based on mode */}
      <AnimatePresence mode="wait">
        {uiMode === 'place' ? (
          <div className="fixed bottom-0 left-0 right-0 flex flex-col">
            {/* Model Selector - Nằm phía trên CameraControls */}
            <ModelSelector
              isExpanded={modelSelectorExpanded}
              onToggle={() => setModelSelectorExpanded(!modelSelectorExpanded)}
              onModelSelect={handleModelSelect}
              selectedModel={selectedModel}
              trackingEnabled={trackingEnabled}
            />

            {/* Camera Controls - Ẩn khi ModelSelector mở rộng */}
            {!modelSelectorExpanded && <CameraControls visible={true} />}
          </div>
        ) : (
          /* Edit Panel */
          <EditPanel
            onDelete={handleDelete}
            onDuplicate={handleDuplicate}
            onDeselect={handleDeselect}
            selectedObjectName={selectedObjectName}
          />
        )}
      </AnimatePresence>

      {/* Toast notifications */}
      <Toaster />
    </>
  )
}

export default App
