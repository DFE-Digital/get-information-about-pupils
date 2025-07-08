using DfE.GIAP.Common.Validation;
using DfE.GIAP.Domain.Search.Learner;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.ViewModels.Search
{
    [ExcludeFromCodeCoverage]
    public class InvalidLearnerNumberSearchViewModel
    {
        #region Navigation properties

        public string SearchAction { get; set; }
        public string InvalidUPNsConfirmationAction { get; set; }

        #endregion Navigation properties

        #region Page Properties

        public string LearnerNumber { get; set; }
        public string LearnerNumberIds { get; set; }

        public IEnumerable<Learner> Learners { get; set; } = new List<Learner>();

        public string SelectedInvalidUPNOption { get; set; }

        #endregion Page Properties
    }
}