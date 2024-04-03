import { SortOrder } from "ant-design-vue/es/table/interface";

export type KaSorterCondition = {
    index: number|null,
    order: SortOrder,
    key: string,
}