<template>
	<a-space>
		<a-space-compact :size="props.size">
			<a-tooltip v-if="props.refresh.isShow !== false" :title="props.refresh.title">
				<a-button @click="props.refresh.onClick">
					<ReloadOutlined />
				</a-button>
			</a-tooltip>
		</a-space-compact>
		<a-space-compact :size="props.size">
			<a-tooltip v-if="props.filter.isShow !== false" :title="props.filter.title">
				<a-button @click="props.filter.onClick" :style="{ color: props.filter.isActive ? props.activateColor : null }">
					<SearchOutlined />
				</a-button>
			</a-tooltip>
			<a-tooltip v-if="props.export.isShow !== false" :title="props.export.title">
				<a-popconfirm
					title="导出"
					ok-text="所有"
					cancel-text="本页"
					@confirm="props.export.onExportAll"
					@cancel="props.export.onExportPage"
				>
					<a-button>
						<DownloadOutlined />
					</a-button>
				</a-popconfirm>
			</a-tooltip>
			<a-tooltip v-if="props.sort.isShow !== false" :title="props.sort.title">
				<a-button @click="props.sort.onClick" :style="{ color: props.sort.isActive ? activateColor : null }">
					<SortAscendingOutlined />
				</a-button>
			</a-tooltip>
		</a-space-compact>
		<a-space-compact :size="props.size">
			<a-tooltip v-if="props.add.isShow !== false" :title="props.add.title">
				<a-button @click="props.add.onClick">
					<PlusOutlined />
				</a-button>
			</a-tooltip>
			<a-tooltip v-if="props.import.isShow !== false" :title="props.import.title">
				<a-button @click="props.import.onClick">
					<UploadOutlined />
				</a-button>
			</a-tooltip>
			<a-tooltip v-if="props.edit.isShow !== false" :title="props.edit.title">
				<a-button @click="props.edit.onClick" :disabled="props.edit.isDisabled">
					<EditOutlined />
				</a-button>
			</a-tooltip>
			<a-tooltip v-if="props.remove.isShow !== false" :title="props.remove.title">
				<a-button @click="props.remove.onClick" :disabled="props.remove.isDisabled">
					<DeleteOutlined />
				</a-button>
			</a-tooltip>
		</a-space-compact>
		<slot name="toolbar"></slot>
	</a-space>
</template>

<script setup lang="ts">
import {
	SearchOutlined,
	ReloadOutlined,
	SortAscendingOutlined,
	PlusOutlined,
	EditOutlined,
	DeleteOutlined,
	DownloadOutlined,
	UploadOutlined,
} from '@ant-design/icons-vue';
import { kaTableToolbarProps } from '.';

const props = defineProps(kaTableToolbarProps());

const onToolbarRefresh = () => {};
const onToolbarAdd = () => {};
const onToolbarEdit = () => {};
</script>

<style scoped></style>
