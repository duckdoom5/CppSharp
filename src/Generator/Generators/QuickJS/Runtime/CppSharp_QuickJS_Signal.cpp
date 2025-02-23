// ----------------------------------------------------------------------------
// <auto-generated>
// This is autogenerated code by CppSharp.
// Do not edit this file or all your changes will be lost after re-generation.
// </auto-generated>
// ----------------------------------------------------------------------------
#include "quickjs.h"
#include <CppSharp_QuickJS.h>
#include <assert.h>

extern "C"
{

    JSClassID classId__Signal;

    // Signal::Signal
    static JSValue callback_method_Signal_Signal(JSContext* ctx, JSValueConst this_val, int argc, JSValueConst* argv)
    {
        // if (argc != 1)
        // {
        //     return JS_ThrowRangeError(ctx, "Unsupported number of arguments");
        // }

    wrap:
        JSValue proto;
        if (JS_IsUndefined(this_val))
            proto = JS_GetClassProto(ctx, classId__Signal);
        else
            proto = JS_GetPropertyStr(ctx, this_val, "prototype");

        if (JS_IsException(proto))
            return proto;

        JSValue __obj = JS_NewObjectProtoClass(ctx, proto, classId__Signal);
        JS_FreeValue(ctx, proto);

        JS_SignalContext* signalCtx = new JS_SignalContext;
        signalCtx->ctx = ctx;
        signalCtx->function = JS_UNDEFINED;
        signalCtx->link = JS_UNDEFINED;

        if (argc >= 1)
        {
            JSValue link = argv[0];
            assert(JS_IsObject(link));
            // JS_FreeValue(ctx, link);
            signalCtx->link = link;
        }

        JS_SetOpaque(__obj, signalCtx);

        return __obj;
    }

    // Signal::connect
    static JSValue callback_method_Signal_connect(JSContext* ctx, JSValueConst this_val, int argc, JSValueConst* argv)
    {
        if (argc < 1 || argc > 1)
        {
            return JS_ThrowRangeError(ctx, "Expected one argument of function type");
        }

        // Signal* instance = (Signal*) JS_GetOpaque(this_val, classId__Signal);

        if (!JS_IsFunction(ctx, argv[0]))
            return JS_ThrowTypeError(ctx, "Unsupported argument type");

        // Connect logic

        JS_SignalContext* signalCtx = (JS_SignalContext*)JS_GetOpaque(this_val, classId__Signal);
        if (signalCtx == nullptr)
            return JS_ThrowTypeError(ctx, "Could not find signal context");

        assert(JS_IsObject(signalCtx->link));

        if (!JS_IsUndefined(signalCtx->function))
            return JS_ThrowRangeError(ctx, "Signal already contains a connected function");

        signalCtx->function = JS_DupValue(ctx, argv[0]);

        JSValue ____ret = JS_NewInt32(ctx, 0);

        return ____ret;
    }

    // Signal::disconnect
    static JSValue callback_method_Signal_disconnect(JSContext* ctx, JSValueConst this_val, int argc, JSValueConst* argv)
    {
        if (argc < 1 || argc > 1)
        {
            return JS_ThrowRangeError(ctx, "Unsupported number of arguments");
        }

        // Signal* instance = (Signal*) JS_GetOpaque(this_val, classId__Signal);

        if (JS_IsNumber(argv[0]))
            goto overload0;

        goto error;

    error:
        return JS_ThrowTypeError(ctx, "Unsupported argument type");

        // bool disconnect(Slot slot) { return 0; }
    overload0:
    {
        int slot;
        if (JS_ToInt32(ctx, (int32_t*)&slot, argv[0]))
            return JS_EXCEPTION;

        // auto __arg0 = (::Slot)slot;
        // bool __ret = instance->disconnect(__arg0);

        JSValue ____ret = JS_NewBool(ctx, 0);

        return ____ret;
    }
    }

    // Signal::isEmpty
    static JSValue callback_method_Signal_isEmpty(JSContext* ctx, JSValueConst this_val, int argc, JSValueConst* argv)
    {
        if (argc > 0)
        {
            return JS_ThrowRangeError(ctx, "Unsupported number of arguments");
        }

        JS_SignalContext* signalCtx = (JS_SignalContext*)JS_GetOpaque(this_val, classId__Signal);

        JSValue ____ret = JS_NewBool(ctx, JS_IsUndefined(signalCtx->function));

        return ____ret;
    }

    static JSValue callback_class__Signal_toString(JSContext* ctx, JSValueConst this_val, int argc, JSValueConst* argv)
    {
        return JS_NewString(ctx, "Signal");
    }

    void finalizer__Signal(JSRuntime* rt, JSValue val)
    {
        JS_SignalContext* signalCtx = (JS_SignalContext*)JS_GetOpaque(val, classId__Signal);
        if (signalCtx == nullptr)
            return;

        if (!JS_IsUndefined(signalCtx->function))
            return JS_FreeValue(signalCtx->ctx, signalCtx->function);

        js_free_rt(rt, signalCtx);

        JS_SetOpaque(val, nullptr);
    }

    static JSClassDef classDef__Signal = {
        .class_name = "Signal",
        .finalizer = finalizer__Signal
    };

    static JSCFunctionListEntry funcDef__Signal[] = {
        JS_CFUNC_DEF("connect", 1, callback_method_Signal_connect),
        JS_CFUNC_DEF("disconnect", 1, callback_method_Signal_disconnect),
        JS_CFUNC_DEF("isEmpty", 0, callback_method_Signal_isEmpty),
        JS_CFUNC_DEF("toString", 0, callback_class__Signal_toString),
    };

    static void register_class__Signal(JSContext* ctx, JSModuleDef* m, bool set, int phase)
    {
        if (!set)
        {
            JS_AddModuleExport(ctx, m, "Signal");
            return;
        }

        if (phase == 0)
        {
            JS_NewClassID(JS_GetRuntime(ctx), &classId__Signal);

            JS_NewClass(JS_GetRuntime(ctx), classId__Signal, &classDef__Signal);

            JSValue proto = JS_NewObject(ctx);
            JS_SetPropertyFunctionList(ctx, proto, funcDef__Signal, sizeof(funcDef__Signal) / sizeof(funcDef__Signal[0]));
            JS_SetClassProto(ctx, classId__Signal, proto);

            JSValue ctor = JS_NewCFunction2(ctx, callback_method_Signal_Signal, "Signal", 1, JS_CFUNC_constructor, 0);
            JS_SetConstructor(ctx, ctor, proto);

            JS_SetModuleExport(ctx, m, "Signal", ctor);
        }
    }

    void register_signal(JSContext* ctx, JSModuleDef* m, bool set, int phase)
    {
        register_class__Signal(ctx, m, set, phase);
    }

} // extern "C"
