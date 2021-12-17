// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace Microsoft.CSharp.RuntimeBinder.Errors
{
    internal static class ErrorFacts
    {
        public static string GetMessage(Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode code)
        {
            string codeStr;
            switch (code)
            {
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadBinaryOps:
                    codeStr = SR.BadBinaryOps;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadIndexLHS:
                    codeStr = SR.BadIndexLHS;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadIndexCount:
                    codeStr = SR.BadIndexCount;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadUnaryOp:
                    codeStr = SR.BadUnaryOp;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NoImplicitConv:
                    codeStr = SR.NoImplicitConv;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NoExplicitConv:
                    codeStr = SR.NoExplicitConv;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_ConstOutOfRange:
                    codeStr = SR.ConstOutOfRange;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AmbigBinaryOps:
                    codeStr = SR.AmbigBinaryOps;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AmbigUnaryOp:
                    codeStr = SR.AmbigUnaryOp;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_ValueCantBeNull:
                    codeStr = SR.ValueCantBeNull;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_WrongNestedThis:
                    codeStr = SR.WrongNestedThis;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NoSuchMember:
                    codeStr = SR.NoSuchMember;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_ObjectRequired:
                    codeStr = SR.ObjectRequired;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AmbigCall:
                    codeStr = SR.AmbigCall;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadAccess:
                    codeStr = SR.BadAccess;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_MethDelegateMismatch:
                    codeStr = SR.MethDelegateMismatch;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AssgLvalueExpected:
                    codeStr = SR.AssgLvalueExpected;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NoConstructors:
                    codeStr = SR.NoConstructors;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_PropertyLacksGet:
                    codeStr = SR.PropertyLacksGet;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_ObjectProhibited:
                    codeStr = SR.ObjectProhibited;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AssgReadonly:
                    codeStr = SR.AssgReadonly;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefReadonly:
                    codeStr = SR.RefReadonly;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AssgReadonlyStatic:
                    codeStr = SR.AssgReadonlyStatic;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefReadonlyStatic:
                    codeStr = SR.RefReadonlyStatic;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AssgReadonlyProp:
                    codeStr = SR.AssgReadonlyProp;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefProperty:
                    codeStr = SR.RefProperty;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_UnsafeNeeded:
                    codeStr = SR.UnsafeNeeded;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadBoolOp:
                    codeStr = SR.BadBoolOp;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_MustHaveOpTF:
                    codeStr = SR.MustHaveOpTF;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_ConstOutOfRangeChecked:
                    codeStr = SR.ConstOutOfRangeChecked;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AmbigMember:
                    codeStr = SR.AmbigMember;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NoImplicitConvCast:
                    codeStr = SR.NoImplicitConvCast;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_InaccessibleGetter:
                    codeStr = SR.InaccessibleGetter;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_InaccessibleSetter:
                    codeStr = SR.InaccessibleSetter;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadArity:
                    codeStr = SR.BadArity;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_TypeArgsNotAllowed:
                    codeStr = SR.TypeArgsNotAllowed;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_HasNoTypeVars:
                    codeStr = SR.HasNoTypeVars;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NewConstraintNotSatisfied:
                    codeStr = SR.NewConstraintNotSatisfied;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_GenericConstraintNotSatisfiedRefType:
                    codeStr = SR.GenericConstraintNotSatisfiedRefType;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_GenericConstraintNotSatisfiedNullableEnum:
                    codeStr = SR.GenericConstraintNotSatisfiedNullableEnum;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_GenericConstraintNotSatisfiedNullableInterface:
                    codeStr = SR.GenericConstraintNotSatisfiedNullableInterface;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_GenericConstraintNotSatisfiedValType:
                    codeStr = SR.GenericConstraintNotSatisfiedValType;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_CantInferMethTypeArgs:
                    codeStr = SR.CantInferMethTypeArgs;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefConstraintNotSatisfied:
                    codeStr = SR.RefConstraintNotSatisfied;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_ValConstraintNotSatisfied:
                    codeStr = SR.ValConstraintNotSatisfied;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AmbigUDConv:
                    codeStr = SR.AmbigUDConv;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BindToBogus:
                    codeStr = SR.BindToBogus;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_CantCallSpecialMethod:
                    codeStr = SR.CantCallSpecialMethod;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_ConvertToStaticClass:
                    codeStr = SR.ConvertToStaticClass;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_IncrementLvalueExpected:
                    codeStr = SR.IncrementLvalueExpected;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadArgCount:
                    codeStr = SR.BadArgCount;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadArgTypes:
                    codeStr = SR.BadArgTypes;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefLvalueExpected:
                    codeStr = SR.RefLvalueExpected;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadProtectedAccess:
                    codeStr = SR.BadProtectedAccess;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BindToBogusProp2:
                    codeStr = SR.BindToBogusProp2;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BindToBogusProp1:
                    codeStr = SR.BindToBogusProp1;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadDelArgCount:
                    codeStr = SR.BadDelArgCount;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadDelArgTypes:
                    codeStr = SR.BadDelArgTypes;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AssgReadonlyLocal:
                    codeStr = SR.AssgReadonlyLocal;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefReadonlyLocal:
                    codeStr = SR.RefReadonlyLocal;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_ReturnNotLValue:
                    codeStr = SR.ReturnNotLValue;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AssgReadonly2:
                    codeStr = SR.AssgReadonly2;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefReadonly2:
                    codeStr = SR.RefReadonly2;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AssgReadonlyStatic2:
                    codeStr = SR.AssgReadonlyStatic2;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefReadonlyStatic2:
                    codeStr = SR.RefReadonlyStatic2;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_AssgReadonlyLocalCause:
                    codeStr = SR.AssgReadonlyLocalCause;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_RefReadonlyLocalCause:
                    codeStr = SR.RefReadonlyLocalCause;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadCtorArgCount:
                    codeStr = SR.BadCtorArgCount;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NonInvocableMemberCalled:
                    codeStr = SR.NonInvocableMemberCalled;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NamedArgumentSpecificationBeforeFixedArgument:
                    codeStr = SR.NamedArgumentSpecificationBeforeFixedArgument;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadNamedArgument:
                    codeStr = SR.BadNamedArgument;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_BadNamedArgumentForDelegateInvoke:
                    codeStr = SR.BadNamedArgumentForDelegateInvoke;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_DuplicateNamedArgument:
                    codeStr = SR.DuplicateNamedArgument;
                    break;
                case Microsoft.CSharp.RuntimeBinder.Errors.ErrorCode.ERR_NamedArgumentUsedInPositional:
                    codeStr = SR.NamedArgumentUsedInPositional;
                    break;
                default:
                    // means missing resources match the code entry
                    Debug.Assert(false, "Missing resources for the error " + code.ToString());
                    codeStr = null;
                    break;
            }

            return codeStr;
        }

        public static string GetMessage(MessageID id)
        {
            return id.ToString();
        }
    }
}
