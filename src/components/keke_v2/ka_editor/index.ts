import { Rule } from 'ant-design-vue/es/form';
import { Component } from 'vue';
import { DefaultOptionType } from 'ant-design-vue/es/select';

export type KaEditorItem = {
	/** 显示排序 */
	index: number;
	/** ant inputNumber precision */
	//precision?: InputNumberProps['precision'];
	/** 内置组件 */
	componentType: 'input' | 'textarea' | 'number' | 'date' | 'select';
	/** 其它属性 */
	attrs?: { [key: string]: any };
	/** 显示标签名称 */
	title: string;
	/** 显示宽度 */
	width: string;
	/** 显示位置 */
	position: 'inline' | 'cling' | 'line';
	/** 提交是否回传 */
	isPost: boolean;
	/** 显示方式 */
	display: 'readonly' | 'hide' | 'show';
	/** select组件选项 */
	options?: KaEditorItemOption[] | ((key: string) => Promise<KaEditorItemOption[]>);
	/** ant select mode */
	// selectMode?: SelectProps['mode'];
	/** select分隔符 */
	selectSplit?: string;
	/** ant formItem rules */
	rules?: Rule[];
	/** 日期格式遮罩 */
	// dateFormat?: DatePickerProps['format'];
	/** ant datepicker showTime */
	// showTime?: DatePickerProps['showTime'];
	/** 自定义值转换 */
	valueConverter?: (value: any) => Promise<any>;
	/** 自定义组件 */
	customComponent?: Component;
	/** 防抖延时 */
	debounceDelay?: number;
	/** 改变值之后 */
	onAfterChange?: (value: any, option?: any) => Promise<void>;
	/** key */
	key: string;
};

export type KaEditorItemGroup = KaEditorItem[];
export type KaEditorRow = KaEditorItemGroup[];
export type KaEditorItemOption = DefaultOptionType & {[key: string]: any };

/** 绘制编辑项目 [ [item,item], [item] ] */
export const render = (editorItemsObj: { [key: string]: KaEditorItem }) => {
	const editorRows = [] as KaEditorRow[];

	// obj -> arr
	const arr = [] as KaEditorItemGroup;
	const keys = Object.keys(editorItemsObj);
	keys.forEach(key => {
		arr.push(editorItemsObj[key]);
	});
	arr.sort((a, b) => a.index - b.index);

	// arr -> rows
	let preRow: KaEditorRow | null = null;
	for (let item of arr) {
		if (item.position === 'line' || item.position == null || preRow === null) {
			preRow = [[item]];
			editorRows.push(preRow);
		} else if (item.position === 'inline') {
			preRow.push([item]);
		} else if (item.position === 'cling') {
			preRow[preRow.length - 1].push(item);
		}
	}

	return editorRows;
};
