import { defineConfig } from "vite";

export default defineConfig({
  define: {
    global: 'globalThis',
  },
  build: {
    target: 'esnext',
    lib: {
      entry: "src/contact-messages-dashboard.element.ts",
      formats: ["es"],
    },
    outDir: "dist",
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },
  base: "/App_Plugins/contactMessages/",
  optimizeDeps: {
    exclude: ['@umbraco-cms/backoffice']
  }
});
