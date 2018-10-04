﻿using System;
using System.Collections.Generic;

namespace gs
{

    public class SingleMaterialFFFPrintGenerator : ThreeAxisPrintGenerator
    {
        GCodeFileAccumulator file_accumulator;
        GCodeBuilder builder;
        SingleMaterialFFFCompiler compiler;

        public SingleMaterialFFFPrintGenerator(PrintMeshAssembly meshes, 
                                      PlanarSliceStack slices,
                                      SingleMaterialFFFSettings settings,
                                      AssemblerFactoryF overrideAssemblerF = null )
        {
            file_accumulator = new GCodeFileAccumulator();
            builder = new GCodeBuilder(file_accumulator);
            AssemblerFactoryF useAssembler = (overrideAssemblerF != null) ?
                overrideAssemblerF : settings.AssemblerType();
            compiler = new SingleMaterialFFFCompiler(builder, settings, useAssembler);
            base.Initialize(meshes, slices, settings, compiler);
        }

        public IEnumerable<string> TotalExtrusionReport {
            get
            {
                return Compiler.GenerateTotalExtrusionReport(Settings);
            }
        }

        protected override GCodeFile extract_result()
        {
            return file_accumulator.File;
        }


    }


}
