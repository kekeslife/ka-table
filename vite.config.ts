import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueJsx from '@vitejs/plugin-vue-jsx';
import { resolve } from 'path';
import { visualizer } from 'rollup-plugin-visualizer';

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [vue(), vueJsx()],
	server: {
		port: 5174,
		cors: true,
		proxy: {
			'/api': {
				target: 'http://localhost:5000/katable', //代理接口
				// target: 'http://192.168.0.100/test', //代理接口
				changeOrigin: true,
				secure: false,
				rewrite: path => path.replace(/^\/api/, ''),
				timeout: 3000,
			},
		},
	},
	define: {
		'process.env': {},
	},
	build: {
		lib: {
			entry: resolve(__dirname, 'src/components/keke_v2/index.ts'),
			name: 'KaTable',
			fileName: 'ka-table',
			formats: ['es', 'iife'],
		},
		rollupOptions: {
			external: ['vue', 'ant-design-vue', 'axios', 'dayjs'],
			// plugins: [visualizer({ open: true })],
			output: {
				globals: {
					vue: 'Vue',
					'ant-design-vue': 'antd',
					'dayjs': 'dayjs',
					'axios': 'axios',
				},
			},
		},
	},
});
