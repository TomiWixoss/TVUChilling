import { create } from 'zustand'

interface OCRStore {
  isOpen: boolean
  studentName: string
  openDialog: (name: string) => void
  closeDialog: () => void
  setStudentName: (name: string) => void
}

export const useOCRStore = create<OCRStore>((set) => ({
  isOpen: false,
  studentName: '',
  openDialog: (name: string) => set({ isOpen: true, studentName: name }),
  closeDialog: () => set({ isOpen: false }),
  setStudentName: (name: string) => set({ studentName: name }),
}))
