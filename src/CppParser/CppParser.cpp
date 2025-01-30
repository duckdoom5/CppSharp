/************************************************************************
*
* CppSharp
* Licensed under the MIT license.
*
************************************************************************/

#include "CppParser.h"

namespace CppSharp { namespace CppParser {

DEF_VECTOR_STRING(CppParserOptions, Arguments)
DEF_VECTOR_STRING(CppParserOptions, CompilationOptions)
DEF_VECTOR_STRING(CppParserOptions, SourceFiles)
DEF_VECTOR_STRING(CppParserOptions, IncludeDirs)
DEF_VECTOR_STRING(CppParserOptions, SystemIncludeDirs)
DEF_VECTOR_STRING(CppParserOptions, Defines)
DEF_VECTOR_STRING(CppParserOptions, Undefines)
DEF_VECTOR_STRING(CppParserOptions, SupportedStdTypes)
DEF_VECTOR_STRING(CppParserOptions, SupportedFunctionTemplates)

ParserResult::~ParserResult()
{
    for (auto Library : Libraries)
    {
        delete Library;
    }
}

DEF_VECTOR(ParserResult, ParserDiagnostic, Diagnostics)
DEF_VECTOR(ParserResult, NativeLibrary*, Libraries)

DEF_VECTOR_STRING(CppLinkerOptions, Arguments)
DEF_VECTOR_STRING(CppLinkerOptions, LibraryDirs)
DEF_VECTOR_STRING(CppLinkerOptions, Libraries)
} }