import { SizeType } from "ant-design-vue/es/config-provider";
import { FileType } from "ant-design-vue/es/upload/interface";
import { PropType } from "vue";

export type KaTableToolbarItem = {
    isShow?: boolean;
    title: string;
    isActive?: boolean;
    isDisabled?: boolean;
    onClick?: () => void;
}

export type KaTableToolbarItemExport = KaTableToolbarItem & {
    onExportPage?: (e: MouseEvent) => void;
    onExportAll?: (e: MouseEvent) => void;
}

export type KaTableToolbarItemImport = KaTableToolbarItem & {
    beforeUpload?:(e:FileType)=>void;
}

export const kaTableToolbarProps = () => ({
    size: { type: String as PropType<SizeType>, default: 'small' },
    refresh: {
        type: Object as PropType<KaTableToolbarItem>,
        default: () => ({
            isShow: true,
            title: '刷新',
            isActive: false,
            isDisabled: false,
        })
    },
    filter: {
        type: Object as PropType<KaTableToolbarItem>,
        default: () => ({
            isShow: true,
            title: '筛选',
            isActive: false,
            isDisabled: false,
        })
    },
    sort: {
        type: Object as PropType<KaTableToolbarItem>,
        default: () => ({
            isShow: true,
            title: '排序',
            isActive: false,
            isDisabled: false,
        })
    },
    add: {
        type: Object as PropType<KaTableToolbarItem>,
        default: () => ({
            isShow: true,
            title: '新增',
            isActive: false,
            isDisabled: false,
        })
    },
    edit: {
        type: Object as PropType<KaTableToolbarItem>,
        default: () => ({
            isShow: true,
            title: '编辑',
            isActive: false,
            isDisabled: false,
        })
    },
    remove: {
        type: Object as PropType<KaTableToolbarItem>,
        default: () => ({
            isShow: true,
            title: '删除',
            isActive: false,
            isDisabled: false,
        })
    },
    export: {
        type: Object as PropType<KaTableToolbarItemExport>,
        default: () => ({
            isShow: true,
            title: '导出',
            isActive: false,
            isDisabled: false,
        })
    },
    import: {
        type: Object as PropType<KaTableToolbarItemImport>,
        default: () => ({
            isShow: true,
            title: '导入',
            isActive: false,
            isDisabled: false,
        })
    },
    activateColor: { type: String, default: '#1677ff' },
});