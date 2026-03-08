import { Bird, GraduationCap, Boxes } from 'lucide-react'

interface ModeSelectorProps {
  onModeSelect: (mode: 'imageTracking' | 'placement') => void
}

export function ModeSelector({ onModeSelect }: ModeSelectorProps) {
  const handleModeClick = (mode: 'imageTracking' | 'placement') => {
    console.log(`[ModeSelector] Selected: ${mode}`)
    
    // Gửi mode selection tới Unity
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onModeSelect', 
        data: mode 
      })
      window.Unity.call(message)
    }
    
    // Callback để switch UI
    onModeSelect(mode)
  }

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-gradient-to-br from-blue-900 via-purple-900 to-pink-900">
      <div className="max-w-md w-full px-6">
        <h1 className="text-4xl font-bold text-white text-center mb-4">
          TVU AR Graduation
        </h1>
        <p className="text-white/80 text-center mb-12">
          Chọn chế độ AR
        </p>

        <div className="space-y-4">
          {/* Mode: Image Tracking (Cúp + Bằng) */}
          <button
            onClick={() => handleModeClick('imageTracking')}
            className="w-full bg-white/10 backdrop-blur-md border-2 border-white/20 rounded-2xl p-6 hover:bg-white/20 active:scale-95 transition-all"
          >
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-full bg-gradient-to-br from-orange-500/20 to-blue-500/20 flex items-center justify-center">
                <div className="relative">
                  <Bird className="w-6 h-6 text-orange-400 absolute -top-2 -left-2" />
                  <GraduationCap className="w-6 h-6 text-blue-400 absolute top-2 left-2" />
                </div>
              </div>
              <div className="flex-1 text-left">
                <h3 className="text-xl font-bold text-white">Cúp & Bằng</h3>
                <p className="text-white/70 text-sm">Quét cúp hoặc bằng tốt nghiệp</p>
              </div>
            </div>
          </button>

          {/* Mode: Placement */}
          <button
            onClick={() => handleModeClick('placement')}
            className="w-full bg-white/10 backdrop-blur-md border-2 border-white/20 rounded-2xl p-6 hover:bg-white/20 active:scale-95 transition-all"
          >
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-full bg-purple-500/20 flex items-center justify-center">
                <Boxes className="w-8 h-8 text-purple-400" />
              </div>
              <div className="flex-1 text-left">
                <h3 className="text-xl font-bold text-white">Trang trí 3D</h3>
                <p className="text-white/70 text-sm">Đặt & chỉnh sửa vật thể</p>
              </div>
            </div>
          </button>
        </div>
      </div>
    </div>
  )
}
