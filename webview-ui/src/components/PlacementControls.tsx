import { Trash2, RotateCcw, Box, Circle, Square } from 'lucide-react'
import { useState } from 'react'

export function PlacementControls() {
  const [selectedPrefab, setSelectedPrefab] = useState(0)

  const prefabs = [
    { id: 0, name: 'Cube', icon: Square },
    { id: 1, name: 'Sphere', icon: Circle },
    { id: 2, name: 'Cylinder', icon: Box },
  ]

  const handlePrefabSelect = (id: number) => {
    setSelectedPrefab(id)
    console.log(`[Placement] Selected prefab: ${id}`)
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onPrefabSelect', 
        data: id.toString() 
      })
      window.Unity.call(message)
    }
  }

  const handleDelete = () => {
    console.log('[Placement] Delete selected')
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onPlacementDelete', 
        data: '' 
      })
      window.Unity.call(message)
    }
  }

  const handleClearAll = () => {
    console.log('[Placement] Clear all')
    
    if (window.Unity && window.Unity.call) {
      const message = JSON.stringify({ 
        method: 'onPlacementClear', 
        data: '' 
      })
      window.Unity.call(message)
    }
  }

  return (
    <>
      {/* Prefab selector - Top left, z-index thấp hơn back button */}
      <div className="fixed top-20 left-6 flex flex-col gap-2 z-40">
        {prefabs.map((prefab) => {
          const Icon = prefab.icon
          return (
            <button
              key={prefab.id}
              onClick={() => handlePrefabSelect(prefab.id)}
              className={`w-14 h-14 rounded-full backdrop-blur-sm flex items-center justify-center active:scale-95 transition-all shadow-lg ${
                selectedPrefab === prefab.id
                  ? 'bg-blue-500/80 border-2 border-white'
                  : 'bg-black/30 border-2 border-white/20'
              }`}
            >
              <Icon className="w-6 h-6 text-white" />
            </button>
          )
        })}
      </div>

      {/* Action buttons - Top right */}
      <div className="fixed top-6 right-6 flex flex-col gap-3">
        {/* Delete selected */}
        <button
          onClick={handleDelete}
          className="w-14 h-14 rounded-full bg-red-500/80 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform shadow-lg"
        >
          <Trash2 className="w-6 h-6 text-white" />
        </button>

        {/* Clear all */}
        <button
          onClick={handleClearAll}
          className="w-14 h-14 rounded-full bg-orange-500/80 backdrop-blur-sm flex items-center justify-center active:scale-95 transition-transform shadow-lg"
        >
          <RotateCcw className="w-6 h-6 text-white" />
        </button>
      </div>

      {/* Instructions */}
      <div className="fixed top-6 left-1/2 -translate-x-1/2">
        <div className="bg-black/50 backdrop-blur-sm px-4 py-2 rounded-full">
          <p className="text-white text-sm">
            Tap để đặt • Tap object để chọn • Pinch để scale
          </p>
        </div>
      </div>
    </>
  )
}
