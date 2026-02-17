/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml",
    "./wwwroot/**/*.html"
  ],
  theme: {
    extend: {
      colors: {
        "jet-black": "#0A0A0A",
        "titanium-grey": "#4A4F57",
        "afterburner-red": "#D12F2F",
        "sky-night-blue": "#0F1C2E",
        "steel-white": "#F2F2F2"
      }
    },
  },
  plugins: [],
}
