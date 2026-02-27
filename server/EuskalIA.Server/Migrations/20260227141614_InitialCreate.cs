using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EuskalIA.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL with IF NOT EXISTS to be safe on databases that were
            // previously created via EnsureCreated (without migration tracking).
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "AigcExercises" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_AigcExercises" PRIMARY KEY,
                    "ExerciseCode" TEXT NOT NULL,
                    "TemplateType" TEXT NOT NULL,
                    "LevelId" TEXT NOT NULL,
                    "Topics" TEXT NOT NULL,
                    "Difficulty" INTEGER NOT NULL,
                    "Status" TEXT NOT NULL,
                    "JsonSchema" TEXT NOT NULL,
                    "SuccessRate" REAL NOT NULL,
                    "CreatedAt" TEXT NOT NULL
                );
            """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Lessons" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Lessons" PRIMARY KEY AUTOINCREMENT,
                    "Title" TEXT NOT NULL,
                    "Topic" TEXT NOT NULL,
                    "Level" INTEGER NOT NULL
                );
            """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Users" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
                    "Username" TEXT NOT NULL,
                    "Nickname" TEXT NOT NULL,
                    "Email" TEXT NOT NULL,
                    "Password" TEXT NOT NULL,
                    "JoinedAt" TEXT NOT NULL,
                    "DeletionCode" TEXT NULL,
                    "CodeExpiration" TEXT NULL,
                    "IsVerified" INTEGER NOT NULL,
                    "VerificationToken" TEXT NULL,
                    "TokenExpiration" TEXT NULL,
                    "Language" TEXT NOT NULL,
                    "IsActive" INTEGER NOT NULL,
                    "DeactivationToken" TEXT NULL,
                    "DeactivationTokenExpiration" TEXT NULL,
                    "Role" TEXT NOT NULL
                );
            """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Exercises" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Exercises" PRIMARY KEY AUTOINCREMENT,
                    "LessonId" INTEGER NOT NULL,
                    "Type" TEXT NOT NULL,
                    "Question" TEXT NOT NULL,
                    "CorrectAnswer" TEXT NOT NULL,
                    "OptionsJson" TEXT NOT NULL,
                    CONSTRAINT "FK_Exercises_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE
                );
            """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "LessonProgresses" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_LessonProgresses" PRIMARY KEY AUTOINCREMENT,
                    "UserId" INTEGER NOT NULL,
                    "LessonId" INTEGER NOT NULL,
                    "CorrectAnswers" INTEGER NOT NULL,
                    "TotalQuestions" INTEGER NOT NULL,
                    "Date" TEXT NOT NULL,
                    CONSTRAINT "FK_LessonProgresses_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_LessonProgresses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );
            """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "Progresses" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Progresses" PRIMARY KEY AUTOINCREMENT,
                    "UserId" INTEGER NOT NULL,
                    "XP" INTEGER NOT NULL,
                    "WeeklyXP" INTEGER NOT NULL,
                    "MonthlyXP" INTEGER NOT NULL,
                    "Streak" INTEGER NOT NULL,
                    "Level" INTEGER NOT NULL,
                    "Txanponak" INTEGER NOT NULL,
                    "Indabak" INTEGER NOT NULL,
                    "LastLessonDate" TEXT NOT NULL,
                    "LastLessonTitle" TEXT NOT NULL,
                    CONSTRAINT "FK_Progresses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );
            """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "UserExerciseAttempts" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_UserExerciseAttempts" PRIMARY KEY,
                    "UserId" INTEGER NOT NULL,
                    "ExerciseId" TEXT NOT NULL,
                    "IsCorrect" INTEGER NOT NULL,
                    "AttemptDate" TEXT NOT NULL,
                    CONSTRAINT "FK_UserExerciseAttempts_AigcExercises_ExerciseId" FOREIGN KEY ("ExerciseId") REFERENCES "AigcExercises" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_UserExerciseAttempts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );
            """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "UserSrsNodes" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_UserSrsNodes" PRIMARY KEY,
                    "UserId" INTEGER NOT NULL,
                    "ConceptId" TEXT NOT NULL,
                    "MasteryLevel" REAL NOT NULL,
                    "RiskFactor" REAL NOT NULL,
                    "LastReviewDate" TEXT NULL,
                    "NextReviewDate" TEXT NULL,
                    CONSTRAINT "FK_UserSrsNodes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );
            """);

            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_Exercises_LessonId" ON "Exercises" ("LessonId");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_LessonProgresses_LessonId" ON "LessonProgresses" ("LessonId");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_LessonProgresses_UserId" ON "LessonProgresses" ("UserId");""");
            migrationBuilder.Sql("""CREATE UNIQUE INDEX IF NOT EXISTS "IX_Progresses_UserId" ON "Progresses" ("UserId");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_UserExerciseAttempts_ExerciseId" ON "UserExerciseAttempts" ("ExerciseId");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_UserExerciseAttempts_UserId" ON "UserExerciseAttempts" ("UserId");""");
            migrationBuilder.Sql("""CREATE INDEX IF NOT EXISTS "IX_UserSrsNodes_UserId" ON "UserSrsNodes" ("UserId");""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "LessonProgresses");

            migrationBuilder.DropTable(
                name: "Progresses");

            migrationBuilder.DropTable(
                name: "UserExerciseAttempts");

            migrationBuilder.DropTable(
                name: "UserSrsNodes");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "AigcExercises");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
