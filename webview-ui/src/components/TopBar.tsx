import { Zap, ZapOff, ScanLine } from 'lucide-react'

interface TopBarProps {
  flashEnabled: boolean
  trackingEnabled: boolean
  onFlashToggle: () => void
  onTrackingToggle: () => void
}

export function TopBar({ 
  flashEnabled, 
  trackingEnabled, 
  onFlashToggle, 
  onTrackingToggle 
}: TopBarProps) {
  return (
    <div className="fixed top-0 left-0 right-0 z-50 pt-safe">
      <div className="flex items-center justify-between px-6 py-4">
        {/* Tracking Toggle - Left */}
        <button
          onClick={onTrackingToggle}
          className={`w-12 h-12 rounded-full backdrop-blur-sm flex items-center justify-center active:scale-95 transition-all shadow-lg ${
            trackingEnabled
              ? 'bg-blue-500/80 border-2 border-white'
              : 'bg-black/30 border-2 border-white/20'
          }`}
        >
          <ScanLine className={`w-6 h-6 ${trackingEnabled ? 'text-white' : 'text-white/60'}`} />
        </button>

        {/* Flash Toggle - Right */}
        <button
          onClick={onFlashToggle}
          className="w-12 h-12 rounded-full bg-black/30 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform shadow-lg"
        >
          {flashEnabled ? (
            <Zap className="w-6 h-6 text-yellow-400 fill-yellow-400" />
          ) : (
            <ZapOff className="w-6 h-6 text-white" />
          )}
        </button>
      </div>
    </div>
  )
}
