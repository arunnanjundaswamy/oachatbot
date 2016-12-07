var gulp = require('gulp');
var sass = require('gulp-sass');
var browserSync = require('browser-sync').create();
var nunjucksRender = require('gulp-nunjucks-render');


gulp.task('sass', function() {
  return gulp.src('scss/*.scss') // Gets all files ending with .scss in app/scss and children dirs
    .pipe(sass())
    .pipe(gulp.dest('css'))
    .pipe(browserSync.reload({
      stream: true
    }));
});

gulp.task('nunjucks', function() {
  // Gets .html and .nunjucks files in pages
  return gulp.src('pages/*.njk')
  .pipe(nunjucksRender({
      path: ['templates']
    }))
  .pipe(gulp.dest('.'))
});

gulp.task('watch', ['browserSync', 'sass', 'nunjucks'], function(){
  gulp.watch('scss/*.scss', ['sass']);
  gulp.watch(['templates/*.njk','pages/*.njk'], ['nunjucks']);
  gulp.watch('*.html', browserSync.reload);
  gulp.watch('js/*.js', browserSync.reload);
});

gulp.task('browserSync', function() {
  browserSync.init({
    server: {
      baseDir: '.'
    },
  })
})
