using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XSSDefenses.Models;

namespace XSSDefenses.XSSDefenses.Builder
{
    public class CspOptionsBuilder
    {
        private readonly CspOptions csptions = new CspOptions();
        internal CspOptionsBuilder() { }
        public CspDirectiveBuilder Defaults { get; set; } = new CspDirectiveBuilder();
        public CspDirectiveBuilder Scripts { get; set; } = new CspDirectiveBuilder();
        public CspDirectiveBuilder Styles { get; set; } = new CspDirectiveBuilder();
        public CspDirectiveBuilder Images { get; set; } = new CspDirectiveBuilder();
        public CspDirectiveBuilder Fonts { get; set; } = new CspDirectiveBuilder();
        public CspDirectiveBuilder Media { get; set; } = new CspDirectiveBuilder();

        internal CspOptions Build()
        {
            this.csptions.Defaults = this.Defaults.Sources;
            this.csptions.Scripts = this.Scripts.Sources;
            this.csptions.Styles = this.Styles.Sources;
            this.csptions.Images = this.Images.Sources;
            this.csptions.Fonts = this.Fonts.Sources;
            this.csptions.Media = this.Media.Sources;
            return this.csptions;
        }
    }
}
