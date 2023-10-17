using System.ComponentModel.DataAnnotations.Schema;

namespace WebAppForMORecSys.Models
{
    /// <summary>
    /// Link between act and question. Question should be used only if user has done act 
    /// </summary>
    public class QuestionAct
    {
        /// <summary>
        /// Unique ID of the link between act and question
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of needed act 
        /// </summary>
        [ForeignKey("Act")]
        public int ActID { get; set; }

        /// <summary>
        /// ID of question that depends on the act
        /// </summary>
        [ForeignKey("Question")]
        public int QuestionID { get; set; }


        /// <summary>
        /// Question that depends on the act
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// Needed act 
        /// </summary>
        public Act Act { get; set; }
    }
}
