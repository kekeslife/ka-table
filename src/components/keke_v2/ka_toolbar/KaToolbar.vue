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
					:title="props.export.title"
					ok-text="所有"
					cancel-text="本页"
					:ok-button-props="confirmBtnProps"
					:cancel-button-props="confirmBtnProps"
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
			<a-tooltip v-if="props.import.isShow !== false">
				<template #title>
					<a-typography-link @click="props.import.downloadTemplate">{{ props.import.title }}</a-typography-link>
				</template>
				<a-upload :file-list="fileList" :before-upload="beforeUpload" :show-upload-list="false" accept=".xlsx">
					<a-button @click="props.import.onClick">
						<UploadOutlined />
					</a-button>
				</a-upload>
			</a-tooltip>
			<a-tooltip v-if="props.edit.isShow !== false" :title="props.edit.title">
				<a-button @click="props.edit.onClick" :disabled="props.edit.isDisabled">
					<EditOutlined />
				</a-button>
			</a-tooltip>
			<a-tooltip v-if="props.remove.isShow !== false" :title="props.remove.title">
				<a-popconfirm
					title="确认删除"
					ok-text="确认"
					:ok-button-props="confirmBtnProps"
					:show-cancel="false"
					@confirm="props.remove.onClick"
					:disabled="props.remove.isDisabled"
				>
					<a-button :disabled="props.remove.isDisabled">
						<DeleteOutlined />
					</a-button>
				</a-popconfirm>
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
import { ref } from 'vue';
import { UploadProps } from 'ant-design-vue';

const confirmBtnProps = { style: { borderRadius: '6px' } };

const props = defineProps(kaTableToolbarProps());

const fileList = ref<UploadProps['fileList']>([]);

const beforeUpload: UploadProps['beforeUpload'] = file => {
	fileList.value = [file];
	
	if (props.import.beforeUpload) {
	    props.import.beforeUpload(file);
	}
	return false;
};

defineExpose({
	getFileList:()=>{return fileList.value},
});
</script>

<style scoped></style>
