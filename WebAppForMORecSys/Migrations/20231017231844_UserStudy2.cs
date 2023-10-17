using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class UserStudy2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_AnswerID",
                table: "UserAnswers",
                column: "AnswerID");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionID",
                table: "UserAnswers",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserID",
                table: "UserAnswers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActs_ActID",
                table: "UserActs",
                column: "ActID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActs_UserID",
                table: "UserActs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionsActs_ActID",
                table: "QuestionsActs",
                column: "ActID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionsActs_QuestionID",
                table: "QuestionsActs",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionID",
                table: "Answers",
                column: "QuestionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionID",
                table: "Answers",
                column: "QuestionID",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionsActs_Acts_ActID",
                table: "QuestionsActs",
                column: "ActID",
                principalTable: "Acts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionsActs_Questions_QuestionID",
                table: "QuestionsActs",
                column: "QuestionID",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActs_Acts_ActID",
                table: "UserActs",
                column: "ActID",
                principalTable: "Acts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActs_Users_UserID",
                table: "UserActs",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Answers_AnswerID",
                table: "UserAnswers",
                column: "AnswerID",
                principalTable: "Answers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Questions_QuestionID",
                table: "UserAnswers",
                column: "QuestionID",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Users_UserID",
                table: "UserAnswers",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionID",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionsActs_Acts_ActID",
                table: "QuestionsActs");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionsActs_Questions_QuestionID",
                table: "QuestionsActs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActs_Acts_ActID",
                table: "UserActs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActs_Users_UserID",
                table: "UserActs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Answers_AnswerID",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Questions_QuestionID",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Users_UserID",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_AnswerID",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_QuestionID",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserID",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserActs_ActID",
                table: "UserActs");

            migrationBuilder.DropIndex(
                name: "IX_UserActs_UserID",
                table: "UserActs");

            migrationBuilder.DropIndex(
                name: "IX_QuestionsActs_ActID",
                table: "QuestionsActs");

            migrationBuilder.DropIndex(
                name: "IX_QuestionsActs_QuestionID",
                table: "QuestionsActs");

            migrationBuilder.DropIndex(
                name: "IX_Answers_QuestionID",
                table: "Answers");
        }
    }
}
