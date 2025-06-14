/// <binding AfterBuild='default' />

var gulp = require('gulp');
const sass = require('gulp-sass')(require('sass'));
var uglify = require('gulp-uglify');
var rename = require("gulp-rename");
var concat = require("gulp-concat");

// Copy jQuery to wwwroot/lib/jquery
gulp.task("copy-jquery", function () {
    return gulp
        .src(["./node_modules/jquery/dist/jquery.js", "./node_modules/jquery/dist/jquery.min.js", "./node_modules/jquery/dist/jquery.min.map"])
        .pipe(gulp.dest("./wwwroot/lib/jquery/dist"));  // Ensure the folder exists
});

// Minify and concatenate scripts
gulp.task("scripts", function () {
    return gulp
        .src(["./node_modules/govuk-frontend/dist/govuk/all.bundle.js", "./Scripts/**/*.js"])
        .pipe(uglify())
        .pipe(concat("giap.min.js"))
        .pipe(gulp.dest("./wwwroot/js/"));
});

// Compile and minify SASS
gulp.task('compile-sass', function () {
    return gulp
        .src("./Styles/Master.scss")
        .pipe(sass({ style: "compressed", loadPaths: ["node_modules"] })
            .on('error', sass.logError))
        .pipe(rename("giap.min.css")) // output filename
        .pipe(gulp.dest('./wwwroot/css'));
});

// Default task (runs all tasks)
gulp.task("default", gulp.series("copy-jquery", "scripts", "compile-sass"));
