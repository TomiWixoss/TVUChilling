import path from "path"
import tailwindcss from "@tailwindcss/vite"
import react from "@vitejs/plugin-react"
import { defineConfig } from "vite"

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  base: './', // Quan trọng: Dùng relative paths cho Unity WebView
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    outDir: '../Assets/StreamingAssets/webview', // Build vào Unity
    emptyOutDir: true,
    assetsDir: 'assets',
  },
})
