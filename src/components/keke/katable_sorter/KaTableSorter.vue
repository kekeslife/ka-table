<template>
	<a-flex gap="middle" justify="center" align="center">
		<a-tree
			@select="onTreeSelect"
			:default-expand-all="true"
			:selected-keys="selectedKeys"
			:tree-data="treeData"
			block-node
			show-icon
		>
			<template #icon="{ column }">
				<template v-if="column.order === 'ascend'">
					<sort-ascending-outlined />
				</template>
				<template v-else-if="column.order === 'descend'">
					<sort-descending-outlined />
				</template>
			</template>
		</a-tree>
		<a-divider type="vertical" style="height: 10rem" />
		<a-space direction="vertical">
			<a-select
				style="width: 100%"
				:value="activeColumn.column.col"
				:options="selectOptions"
				@change="onSelectChange"
			></a-select>
			<a-space>
				<a-segmented
					:disabled="!activeColumn.column.col"
					v-model:value="activeColumn.column.order"
					:options="[
						{ payload: { icon: SortAscendingOutlined }, value: 'ascend' },
						{ payload: { icon: SortDescendingOutlined }, value: 'descend' },
					]"
				>
					<template #label="{ payload }">
						<div style="font-size: 1rem">
							<component :is="payload.icon" />
						</div>
					</template>
				</a-segmented>
				<a-space-compact>
					<a-button :disabled="!activeColumn.column.col" :icon="h(ArrowUpOutlined)" @click="onMoveUp"></a-button>
					<a-button :disabled="!activeColumn.column.col" :icon="h(ArrowDownOutlined)" @click="onMoveDown"></a-button>
				</a-space-compact>
				<a-button :disabled="!activeColumn.column.col" :icon="h(DeleteOutlined)" @click="onRemove"></a-button>
			</a-space>
		</a-space>
	</a-flex>
	<div>
		<div>{{ JSON.stringify(activeColumn.column) }}</div>
		<div>{{ JSON.stringify(columns) }}</div>
		<a-button
			@click="
			() => {
				activeColumn.column = columns![0];
			}
		"
			>test1</a-button
		>
	</div>
</template>

<script setup lang="ts">
import { PropType, computed, onBeforeMount, reactive, ref, h } from 'vue';
import {
	SortAscendingOutlined,
	SortDescendingOutlined,
	DeleteOutlined,
	ArrowUpOutlined,
	ArrowDownOutlined,
} from '@ant-design/icons-vue';
import { KaSorterCondition } from '.';

const props = defineProps({
	//columns: { type: Array as PropType<KaSorterColumn[]> },
});
/** 所有字段 */
const columns = defineModel<KaSorterCondition[]>();

/** 字段选择项 */
const selectOptions = computed(() => columns.value?.map(c => ({ label: c.title, value: c.col, column: c })));

/** 树项目 */
const treeData = computed(() => {
	console.log('trrData');
	return columns.value?.filter(c => c.order).map(c => ({ title: c.title, key: c.col, column: c }));
});

/** 当前字段 */
const activeColumn = reactive({ column: { col: '', order: '', title: '' } as KaSorterCondition });

/** 点击树项目 */
const onTreeSelect = (keys: string[], { selected, node }: { selected: boolean; node: any }) => {
	if (selected) {
		activeColumn.column = node.column;
		selectedKeys.value = keys;
	}
};

/** 选择字段 */
const onSelectChange = (col: string, option: any) => {
	activeColumn.column = option.column;
	selectedKeys.value = [col];
};

/** 树选中的项目 */
const selectedKeys = ref<string[]>([]);

const onMoveUp = () => {
	if (activeColumn.column.order == null) return;
	const curIndex = columns.value!.findIndex(c => c.col == activeColumn.column.col) ?? -1;
	if (curIndex <= 0) return;
	const tarIndex = curIndex - 1;
	[columns.value![curIndex], columns.value![tarIndex]] = [columns.value![tarIndex], columns.value![curIndex]];
};
const onMoveDown = () => {
	if (activeColumn.column.order == null) return;
	const curIndex = columns.value!.findIndex(c => c.col == activeColumn.column.col) ?? -1;
	const treeNodesLen = treeData.value?.length ?? 0;
	if (curIndex === treeNodesLen - 1) return;
	const tarIndex = curIndex + 1;
	[columns.value![curIndex], columns.value![tarIndex]] = [columns.value![tarIndex], columns.value![curIndex]];
};
const onRemove = () => {
    activeColumn.column.order = null;
}

onBeforeMount(() => {});
</script>

<style scoped></style>
