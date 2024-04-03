<template>
	<ka-drag-list ref="$drag">
		<template #renderItem="{ item }">【{{ item.title }}】</template>
		<template #renderAction="{ item }"><a>edit</a></template>
	</ka-drag-list>
	<a-button @click="$drag?.setSorter(dragData)">setItems</a-button>
	<a-button @click="console.log($drag?.getSorter())">getItems</a-button>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import KaDragList from './KaDragList.vue';

const $drag = ref<InstanceType<typeof KaDragList>>();

const dragData = ref([
	{ title: '公文号', col: 'code' },
	{ title: '序号', col: 'item' },
]);
</script>

<style scoped></style>
