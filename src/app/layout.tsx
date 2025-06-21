import type { Metadata } from 'next'
import { Noto_Sans_JP } from 'next/font/google'
import './globals.css'

const notoSansJP = Noto_Sans_JP({
  subsets: ['latin'],
  weight: ['400', '700'],
  variable: '--font-noto-sans-jp',
})

export const metadata: Metadata = {
  title: 'お絵描き当てゲーム',
  description: 'リアルタイムお絵描き当てゲーム',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="ja">
      <body className={`${notoSansJP.variable} font-noto-sans bg-background text-text`}>
        {children}
      </body>
    </html>
  )
}
