import { defineComponent, h, reactive,toRef } from "vue";
import { Button,Input } from "ant-design-vue";
import lodash from 'lodash';

const FormTsx = defineComponent({
    name:'form-tsx',

    // components: {
    //     Button
    // },

    setup(props, ctx) {
        const obj = reactive({
            a:1,
            b:{
                bb:1
            }
        });
        const arr = ['a','b.bb'];
        const objB = toRef(obj,'b');
        const objRefs = toRef(obj.b,'bb');

        return ()=>(
            // arr.map(a=><input v-model={} />)
            <>
            <Input v-model:value={objRefs.value}></Input>
            <span>{obj.b.bb}</span>
            </>
        )
    },
});


export default FormTsx;