import { KaEditorItem, KaEditorItemOption } from "../ka_editor";

export type KaFilterCondition = {
    key?: string;
    opt?: 'eq' | 'neq' | 'gte' | 'ge' | 'lt' | 'lte' | 'beg' | 'end' | 'like' | 'in' | 'nu' | 'nnu';
    val?: any;
    bool: 'and' | 'or';
    children?: KaFilterCondition[];
}

export type KaFilterItem = KaFilterCondition & {
    valOptions?: KaEditorItemOption[];
    valComponent?: 'input' | 'number' | 'date' | 'select';
    valAttrs?: { [key: string]: any };
    optOptions: { label: string, value: string }[];
    col?: KaFilterCol;
    children?: KaFilterItem[];
}

export type KaFilterCol = {
    key: string;
    title: string;
    componentType: KaEditorItem['componentType'];
    options?: KaEditorItem['options'];
    debounceDelay?: KaEditorItem['debounceDelay'];
    attrs?: { [key: string]: any };
    valueConverter?: KaEditorItem['valueConverter'];
    // selectSplit?: string;
}

export const createEmptyItem = () => {
    return {
        key: '',
        val: '',
        bool: 'and',
    } as KaFilterItem;
};
