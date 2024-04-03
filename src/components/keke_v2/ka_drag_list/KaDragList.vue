<template>
	<a-list size="small" bordered :data-source="list">
		<template #renderItem="{ item, index }">
			<a-list-item
				:class="{
					'drag-over-top': index === overIndex && index < dragIndex,
					'drag-over-bottom': index === overIndex && index > dragIndex,
					disabled: item.disabled,
				}"
				@dragstart="onDragStart($event, index)"
				@dragover="onDragOver($event, index)"
				@drop="onDrop($event, index)"
			>
				<template #actions>
					<slot name="renderAction" :item="item"></slot>
				</template>
				<a-flex gap="small" style="width: 100%">
					<div :class="{ hide: item.disabled }" class="move-icon" @mouseenter="openDrag" @mouseleave="closeDrag">
						<holder-outlined />
					</div>
					<slot name="renderItem" :item="item">{{ item.title }}</slot>
				</a-flex>
			</a-list-item>
		</template>
	</a-list>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { HolderOutlined } from '@ant-design/icons-vue';
// const list = defineModel<any[]>();

const list = ref<any[]>([]);

const overIndex = ref(-1);
const dragIndex = ref(-1);
const dropIndex = ref(-1);

const onDragStart = (_e: DragEvent, index: number) => {
	dragIndex.value = index;
};
const onDragOver = (e: DragEvent, index: number) => {
	e.preventDefault();
	overIndex.value = index;
};
const onDrop = (e: DragEvent, index: number) => {
	e.preventDefault();

	dropIndex.value = index;

	let fromIndex = dragIndex.value;
	let toIndex = index;

	if (fromIndex !== toIndex) {
		list.value!.splice(toIndex, 0, list.value!.splice(fromIndex, 1)[0]);
	}

	overIndex.value = -1;
};

const openDrag = (e: MouseEvent) => {
	const $li = (e.target as HTMLElement)!.closest('.ant-list-item');
	$li?.setAttribute('draggable', 'true');
};
const closeDrag = (e: MouseEvent) => {
	const $li = (e.target as HTMLElement)!.closest('.ant-list-item');
	$li?.setAttribute('draggable', 'false');
};

const setSorter = (arr: any[]) => {
	// list.value = lodash.cloneDeep(arr);
	list.value = arr;
};

const getSorter = () => {
	// return lodash.cloneDeep(list.value);
	return list.value;
};

defineExpose({
	setSorter,
	getSorter,
});
</script>

<style scoped>
.move-icon {
	cursor: move;
}
.drag-over-top {
	/* background-color: #4096ff55; */
	border-block-start: 1px solid #4096ff !important;
}
.drag-over-bottom {
	/* background-color: #4096ff55; */
	border-block-end: 1px solid #4096ff !important;
}
.hide {
	visibility: hidden;
}
.disabled {
	color: #33333399;
}
</style>
