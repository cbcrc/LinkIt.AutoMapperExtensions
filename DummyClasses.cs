using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource {
    public interface ILinkedSource<T> {
        T Model { get; }
    }

    public class LinkedSource : ILinkedSource<AModel> {
        public AModel Model { get; set; }
        public AReference Reference { get; set; }
    }

    public class AModel {
        public string X { get; set; }
        public string Y { get; set; }
        public int Reference { get; set; }
    }

    public class AReference {
        public string A { get; set; }
        public string B { get; set; }
    }

    public class ADto {
        public string X { get; set; }
        public string Y { get; set; }
        public AReferenceDto Reference { get; set; }
    }

    public class AReferenceDto {
        public string A { get; set; }
        public string B { get; set; }
    }
}
