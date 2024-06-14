import * as lodash from 'lodash-es';
import { KaTableCol, KaTableCols, KaTableExportPar, KaTableImportCol, KaTableListCol } from '.';
import KaTable from './KaTable.vue';
import { KaEditorItem } from '../ka_editor';
import { KaFilterCol } from '../ka_filter';
import { KaSorterCondition } from '../ka_sorter';

// #region init

export const initPorps = (props: InstanceType<typeof KaTable>['$props']) => {
	if (props.toolbar === undefined) {
		props.toolbar = {
			hasAdd: true,
			hasEdit: true,
			hasExport: false,
			hasFilter: true,
			hasImport: false,
			hasRefresh: true,
			hasRemove: true,
			hasSort: true,
		};
	} else if (props.toolbar === null) {
		props.toolbar = {
			hasAdd: false,
			hasEdit: false,
			hasExport: false,
			hasFilter: false,
			hasImport: false,
			hasRefresh: false,
			hasRemove: false,
			hasSort: false,
		};
	} else {
		if (props.toolbar.hasAdd == null) props.toolbar.hasAdd = true;
		if (props.toolbar.hasEdit == null) props.toolbar.hasEdit = true;
		if (props.toolbar.hasExport == null) props.toolbar.hasExport = false;
		if (props.toolbar.hasFilter == null) props.toolbar.hasFilter = true;
		if (props.toolbar.hasImport == null) props.toolbar.hasImport = false;
		if (props.toolbar.hasRefresh == null) props.toolbar.hasRefresh = true;
		if (props.toolbar.hasRemove == null) props.toolbar.hasRemove = true;
		if (props.toolbar.hasSort == null) props.toolbar.hasSort = true;
	}

	if (!props.initFilterConditions) {
		props.initFilterConditions = [];
	}

	if (!props.frozenFilterConditions) {
		props.frozenFilterConditions = [];
	}

	if (!props.initSorterConditions) {
		props.initSorterConditions = [];
	}

	for (const colName in props.columns) {
		initCols(props.columns[colName], [colName]);
	}
};

/** 初始化col */
const initCols = (colObj: KaTableCol | { [key: string]: KaTableCol }, path: string[]) => {
	// if (Object.hasOwn(colObj, 'title')) {
	if (colObj.hasOwnProperty('title')) {
		if (lodash.isString(colObj.title)) {
			initCol(colObj as KaTableCol, [...path]);
			return;
		}
	}
	if (lodash.isString(colObj)) throw new Error(`column配置错误:(${colObj})`);
	for (const key in colObj) {
		const subObj = (colObj as { [key: string]: KaTableCol })[key];
		initCols(subObj, [...path, key]);
	}
};
const initCol = (col: KaTableCol, path: string[]) => {
	const key = path.join('.');
	col.key = key;
	col._katableIsCol = true;

	// dbInfo
	if (!col.dbInfo) {
		col.dbInfo = {
			dataType: 'string',
		};
	} else {
		if (col.dbInfo.dataType == null) {
			col.dbInfo.dataType = 'string';
		} else if (col.dbInfo.dataType === 'date') {
			if (col.dbInfo.dateFormat === undefined) {
				col.dbInfo.dateFormat = 'YYYY/MM/DD';
			}
		}
	}

	//listInfo
	if (col.listInfo) {
		if (col.listInfo.index === undefined) {
			throw `列(${col.key})未设置(listInfo.index)`;
		}
		if (!col.listInfo.title) {
			col.listInfo.title = col.title.toString() || lodash.last(path);
		}
		if (col.listInfo.width === undefined) {
			col.listInfo.width = 100;
		}
	}


	// 编辑
	if (col.editorInfo) {
		if (col.editorInfo.componentType === 'select') {
			if (!col.editorInfo.options) {
				col.editorInfo.options = col.listInfo?.options as any;
			}
		}
		if(col.editorInfo.isPost == null){
			col.editorInfo.isPost = true;
		}
	}

	// 筛选
	if (!col.filterInfo) {
		col.filterInfo = {
			isFilter: col.editorInfo?.isPost === false ? false : true,
		};
	}
	if (!col.filterInfo.width) {
		col.filterInfo.width = col.editorInfo?.width || col.listInfo?.width || 100;
	}
	// if (props.initFilterConditions && props.initFilterConditions.length > 0) {
	//     if (props.initFilterConditions.some(c => c.col === col.key)) {
	//         if (!col.filterInfo) {
	//             col.filterInfo = {};
	//         }
	//         col.filterInfo.isFilter = true;
	//     }
	// }
	
	// 排序
	// if (!col.sortInfo) {
	// 	if (col.editorInfo?.isPost) {
	// 		col.sortInfo = {
	// 			index: null,
	// 		};
	// 	}
	// }

	// 导出
	if (col.exportInfo == null) {
		if (col.listInfo?.index != null) {
			col.exportInfo = {
				index: col.listInfo.index,
			};
		}
	}
	if (col.exportInfo) {
		if (!col.title) {
			col.title = col.listInfo?.title as string;
		}
	}
};
/** 初始化所有字段 */
export const createAllCols = (columns: KaTableCols) => {
	const result: { [key: string]: KaTableCol } = {};

	const itemHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			const _c = col as KaTableCol;
			result[_c.key!] = _c;
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				itemHandle(_col);
			}
		}
	};

	for (const colName in columns) {
		itemHandle(columns[colName]);
	}

	return result;
};
/** 初始化editor项供编辑组件使用 */
export const createEditorItemsObj = (columns: KaTableCols): { [key: string]: KaEditorItem } => {
	const editorItemsObj = {} as { [key: string]: KaEditorItem };

	const itemHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			let _c = col as KaTableCol;
			// 整理编辑项
			if (_c.editorInfo?.index != null) {
				if (_c.key == null) throw `${_c.title}没有_key`;
				const editorItem: KaEditorItem = {
					key: _c.key,
					index: _c.editorInfo.index,
					title: getFirstNotNull([_c.editorInfo.title, _c.title, _c.key]),
					width: _c.editorInfo.width || '100%',
					position: _c.editorInfo.position || 'line',
					isPost: _c.editorInfo.isPost == null ? true : _c.editorInfo.isPost,
					display: _c.editorInfo.display || 'show',
					rules: _c.editorInfo.rules,
					componentType: 'input',
					valueConverter: _c.editorInfo.valueConverter,
					customComponent: _c.editorInfo.customComponent,
					debounceDelay: _c.editorInfo.debounceDelay,
					onAfterChange: _c.editorInfo.onAfterChange,
				};
				editorItem.attrs = {};
				if (!_c.editorInfo.componentType) {
					switch (_c.dbInfo?.dataType) {
						case 'date':
						case 'number':
							editorItem.componentType = _c.dbInfo.dataType;
							break;
						default:
							editorItem.componentType = 'input';
							break;
					}
				} else {
					editorItem.componentType = _c.editorInfo.componentType;
				}
				// 日期组件
				if (editorItem.componentType === 'date') {
					editorItem.attrs['format'] = _c.dbInfo?.dateFormat || 'YYYY-MM-DD';
					editorItem.attrs['showNow'] = true;
					editorItem.attrs['showToday'] = true;
					//时间遮罩
					editorItem.attrs['showTime'] = createTimeFormat(editorItem.attrs['format']);
				}
				// 数字组件
				else if (editorItem.componentType === 'number') {
					editorItem.attrs['precision'] = _c.editorInfo.precision;
				}
				// 选择组件
				else if (editorItem.componentType === 'select') {
					if (!_c.editorInfo.options) {
						console.error(`${_c.key}缺少editorInfo.options`);
					}
					editorItem.options = _c.editorInfo.options;
					editorItem.attrs['showSearch'] = true;
					editorItem.attrs['mode'] = _c.editorInfo.selectMode || 'combobox';
					if (editorItem.attrs['mode'] !== 'combobox') {
						editorItem.selectSplit = _c.editorInfo.selectSplit || ';';
					}
					if (lodash.isArray(editorItem.options)) {
						editorItem.attrs['options'] = editorItem.options;
					}
				}

				Object.assign(editorItem.attrs, _c.editorInfo.attrs);
				editorItemsObj[_c.key] = editorItem;
			}
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				itemHandle(_col);
			}
		}
	};

	for (const colName in columns) {
		itemHandle(columns[colName]);
	}

	return editorItemsObj;
};

/** 初始化筛选字段供筛选组件使用 */
export const createFilterCols = (columns: KaTableCols, editorObj: { [key: string]: KaEditorItem }) => {
	const result = [] as KaFilterCol[];

	const itemHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			let tableCol = col as KaTableCol;
			let editor = editorObj[tableCol.key!];
			// 整理筛选
			if (tableCol.filterInfo?.isFilter) {
				const filterCol: KaFilterCol = {
					key: tableCol.key!,
					title: getFirstNotNull([tableCol.listInfo?.title, tableCol.title, tableCol.key!]),
					componentType: 'input',
					valueConverter: tableCol.filterInfo.valueConverter,
					debounceDelay: editor?.debounceDelay,
					width: tableCol.filterInfo.width?.toString(),
				};
				filterCol.attrs = {};
				if (!editor) {
					switch (tableCol.dbInfo?.dataType) {
						case 'date':
						case 'number':
							filterCol.componentType = tableCol.dbInfo.dataType;
							break;
						default:
							filterCol.componentType = 'input';
							break;
					}
				} else {
					filterCol.componentType = editor.componentType;
				}
				// 日期组件
				if (filterCol.componentType === 'date') {
					filterCol.attrs['format'] = tableCol.dbInfo?.dateFormat || 'YYYY-MM-DD';
					filterCol.attrs['showNow'] = true;
					filterCol.attrs['showToday'] = true;
					//时间遮罩
					filterCol.attrs['showTime'] = createTimeFormat(filterCol.attrs['format']);
				}
				// 数字组件
				else if (filterCol.componentType === 'number') {
					filterCol.attrs['precision'] = editor?.attrs?.precision;
				}
				// 选择组件
				else if (filterCol.componentType === 'select') {
					filterCol.options =
						tableCol.filterInfo.options || editor?.options || (tableCol.listInfo?.options as KaEditorItem['options']);
					filterCol.attrs['showSearch'] = true;
					filterCol.attrs['mode'] = editor?.attrs?.mode || 'combobox';
					if (lodash.isArray(filterCol.options)) {
						filterCol.attrs['options'] = filterCol.options;
					} else {
						filterCol.attrs['options'] = [];
					}
				}

				Object.assign(filterCol.attrs, tableCol.filterInfo.attrs);
				result.push(filterCol);
			}
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				itemHandle(_col);
			}
		}
	};

	for (const colName in columns) {
		itemHandle(columns[colName]);
	}

	return result;
};
/** 初始化汇出exportInfos */
export const createExportCols = (columns: KaTableCols) => {
	const result: KaTableExportPar['cols'] = [];

	const itemHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			const _c = col as KaTableCol;
			if (_c.exportInfo?.index != null) {
				result.push({
					key: _c.key as string,
					title: getFirstNotNull([_c.exportInfo.title, _c.title, _c.key]),
					formula: _c.exportInfo.formula,
					dateFormat: _c.dbInfo?.dataType === 'date' ? _c.dbInfo?.dateFormat || 'YYYY-MM-DD' : undefined,
					index: _c.exportInfo.index,
				});
			}
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				itemHandle(_col);
			}
		}
	};

	for (const colName in columns) {
		itemHandle(columns[colName]);
	}

	result.sort(sortNullLast('index'));

	return result;
};
/** 初始化汇入 */
export const createImportCols = (columns: KaTableCols) => {
	const result: KaTableImportCol[] = [];

	const itemHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			const _c = col as KaTableCol;
			if (_c.importInfo?.index != null) {
				result.push({
					key: _c.key as string,
					dataIndex: _c.key as string,
					title: getFirstNotNull([_c.importInfo.title, _c.title, _c.key]),
					index: _c.importInfo.index,
					width: _c.listInfo?.width,
				});
			}
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				itemHandle(_col);
			}
		}
	};

	for (const colName in columns) {
		itemHandle(columns[colName]);
	}

	result.sort(sortNullLast('index'));

	return result;
};

// #region 排序
/** 初始化排序字段供排序组件使用 */
export const createSorterConditions = (columns: KaTableCols): KaSorterCondition[] => {
	const result = [] as KaSorterCondition[];

	const itemHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			let tableCol = col as KaTableCol;
			// 整理筛选
			if (tableCol.sortInfo) {
				const sorterCondition: KaSorterCondition = {
					key: tableCol.key!,
					order: null,
					index: tableCol.sortInfo.index,
					// title: tableCol.listInfo?.title?.toString() || tableCol.title || tableCol.key!,
				};

				result.push(sorterCondition);
			}
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				itemHandle(_col);
			}
		}
	};

	for (const colName in columns) {
		itemHandle(columns[colName]);
	}

	resetSorterIndex(result);

	return result;
};
/** 初始化排序字段对象 */
export const createSorterObj = (columns: KaTableCols): { [key: string]: string } => {
	const result = {} as { [key: string]: string };

	const itemHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			let tableCol = col as KaTableCol;
			// 整理筛选
			if (tableCol.sortInfo) {
				result[tableCol.key!] = tableCol.listInfo?.title?.toString() || tableCol.title || tableCol.key!;
			}
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				itemHandle(_col);
			}
		}
	};

	for (const colName in columns) {
		itemHandle(columns[colName]);
	}

	return result;
};
/** 重新整理sorter索引 */
export const resetSorterIndex = (sorterConditions: KaSorterCondition[]) => {
	sorterConditions.sort(sortNullLast('index'));
	let index = 1;
	for (const sorterCondition of sorterConditions) {
		if (sorterCondition.index != null) {
			sorterCondition.index = index;
			index++;
		}
	}
};
/** 设置antd table排序 */
export const setAntSorters = (antCols: KaTableListCol[], sorterConditions: KaSorterCondition[]) => {
	for (const antCol of antCols) {
		setAntSorter(
			antCol,
			sorterConditions.find(item => item.key === antCol.key)
		);
	}
};
const setAntSorter = (antCol: KaTableListCol, sorterCondition?: KaSorterCondition) => {
	if (sorterCondition) {
		if (sorterCondition.index !== null) {
			antCol.sortOrder = sorterCondition.order;
			antCol.sorter = { multiple: sorterCondition.index };
		} else {
			antCol.sortOrder = null;
			antCol.sorter = { multiple: 0 };
		}
	} else {
		antCol.sortOrder = null;
		antCol.sorter = false;
	}
};
// #endregion 排序

/** 初始化显示字段供antd table使用 */
export const createAntCols = (columns: KaTableCols): KaTableListCol[] => {
	const result = [] as KaTableListCol[];

	const itemHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			let tableCol = col as KaTableCol;

			if (tableCol.listInfo) {
				const antCol: KaTableListCol = {
					dataIndex: tableCol.key!.split('.'),
					key: tableCol.key,
					title: getFirstNotNull([tableCol.listInfo.title, tableCol.title, tableCol.key!]),
					width: tableCol.listInfo.width || '100',
					customCell: tableCol.listInfo.customCell,
					customHeaderCell: tableCol.listInfo.customHeaderCell,
					customRender: tableCol.listInfo.customRender,
					align: tableCol.listInfo.align || (tableCol.dbInfo?.dataType === 'number' ? 'right' : 'left'),
					ellipsis: tableCol.listInfo.ellipsis || true,
					index: tableCol.listInfo.index,
					options: tableCol.listInfo.options,
				};
				Object.assign(antCol, tableCol.listInfo.attrs);

				result.push(antCol);
			}
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				itemHandle(_col);
			}
		}
	};

	for (const colName in columns) {
		itemHandle(columns[colName]);
	}

	result.sort(sortNullLast('index'));

	return result;
};

/** 设置antd table筛选 */
export const createAntFilters = (antCols: KaTableListCol[], filterCols: KaFilterCol[]) => {
	// for (const antCol of antCols) {
	// 	setAntFilter(
	// 		antCol,
	// 		filterConditions.find(item => item.key === antCol.key)
	// 	);
	// }
	for (const col of filterCols) {
		const antCol = antCols.find(item => item.key === col.key);
		if (antCol) {
			antCol.filteredValue = [];
			antCol.customFilterDropdown = true;
		}
	}
};

// #endregion init

const getFirstNotNull = (arr: any[]) => arr.find(item => item !== null && item !== undefined);

const createTimeFormat = (format: string) => {
	const arr = [] as number[];
	const hMatch = format.match(/h+/i);
	if (hMatch) {
		arr.push(hMatch.index!);
		arr.push(hMatch.index! + hMatch[0].length);
	}
	const mHatch = format.match(/m+/);
	if (mHatch) {
		arr.push(mHatch.index!);
		arr.push(mHatch.index! + mHatch[0].length);
	}
	const sHatch = format.match(/s+/);
	if (sHatch) {
		arr.push(sHatch.index!);
		arr.push(sHatch.index! + sHatch[0].length);
	}
	if (arr.length === 0) {
		return false;
	} else {
		return {
			format: format.substring(Math.min(...arr), Math.max(...arr) + 1),
		};
	}
};

export const sortNullLast = (key?: string) => {
	if (key)
		return (a: any, b: any) =>
			((a[key] == null) as any) - ((b[key] == null) as any) || +(a[key] > b[key]) || -(a[key] < b[key]);
	else return (a: any, b: any) => ((a == null) as any) - ((b == null) as any) || +(a > b) || -(a < b);
};

export const qsStringify = (obj: { [key: string]: any }) => {
	const par = new URLSearchParams();
	for (const key in obj) {
		if (obj[key] !== null) {
			par.append(key, obj[key]);
		}
	}
	return par;
};
