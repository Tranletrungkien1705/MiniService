import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
// Build ra ../wwwroot (được ASP.NET serve tĩnh); base '/' để asset load đúng.
export default defineConfig({
  plugins: [react()],
  base: '/',
  build: { outDir: '../wwwroot', emptyOutDir: false, assetsDir: 'assets' }
})
