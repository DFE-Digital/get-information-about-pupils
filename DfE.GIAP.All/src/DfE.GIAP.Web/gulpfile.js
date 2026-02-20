/// <binding AfterBuild='default' />

const gulp = require("gulp");
const uglify = require("gulp-uglify");

// Correct GOV.UK Frontend v5+ paths
const paths = {
    css: "node_modules/govuk-frontend/dist/govuk/govuk-frontend.min.css",
    js: "node_modules/govuk-frontend/dist/govuk/govuk-frontend.min.js",
    assets: "node_modules/govuk-frontend/dist/govuk/assets/**/*"
};

// Copy GOV.UK CSS → wwwroot/css
gulp.task("govuk-css", function () {
    return gulp.src(paths.css)
        .pipe(gulp.dest("wwwroot/css"));
});

// Copy GOV.UK JS → wwwroot/js
gulp.task("govuk-js", function () {
    return gulp.src(paths.js)
        .pipe(gulp.dest("wwwroot/js"));
});

// Copy GOV.UK assets → wwwroot/assets/govuk
gulp.task("govuk-assets", function () {
    return gulp.src(paths.assets)
        .pipe(gulp.dest("wwwroot/assets"));
});

// Build everything
gulp.task("govuk", gulp.parallel("govuk-css", "govuk-js", "govuk-assets"));

// Default task
gulp.task("default", gulp.series("govuk"));
