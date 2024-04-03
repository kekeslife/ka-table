import { KaTableOptionItem } from '../katable/index';

export type KaTableFilterCondition = {
    col?: string;
    opt?: 'eq' | 'neq' | 'gte' | 'ge' | 'lt' | 'lte' | 'beg' | 'end' | 'like' | 'in' | 'nu' | 'nnu';
    val?: any;
    bool: 'and' | 'or';
    type?: 'number' | 'date' | 'select' | 'string';
    disabled?: boolean;
    colLength?: number;
    multiple?: boolean;
    valOptions?: KaTableOptionItem[];
    children?: KaTableFilterCondition[];
    dateFormat?: string;
    options?:KaTableOptionItem[];
}

export type KaTableFilterCol = {
    col: string;
    title: string;
    type?: 'number' | 'string' | 'date';
    colLength?: number;
    valOptions?: KaTableOptionItem[];
    dateFormat?: string;
}