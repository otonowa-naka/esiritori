/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        primary: '#4A90E2',
        accent: '#F5A623',
        background: '#F8F9FA',
        text: '#333333',
      },
      fontFamily: {
        'noto-sans': ['Noto Sans JP', 'sans-serif'],
      },
      fontSize: {
        'heading': '24px',
        'subheading': '20px',
        'body': '16px',
        'small': '14px',
      },
    },
  },
  plugins: [
    require('@tailwindcss/typography'),
  ],
};
