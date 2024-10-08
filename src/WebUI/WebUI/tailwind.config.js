/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Areas/**/*.cshtml',
    './Views/**/*.cshtml'
  ],
  theme: {
    extend: {},
  },
  plugins: [
    require('@tailwindcss/forms')
  ],
}

