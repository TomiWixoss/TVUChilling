import { CameraControls } from '@/components/CameraControls'
import { Toaster } from '@/components/ui/sonner'

export function App() {
  return (
    <>
      {/* Camera controls - Hiện ở dưới cùng như app Camera */}
      <CameraControls />

      {/* Toast notifications */}
      <Toaster />
    </>
  )
}

export default App
