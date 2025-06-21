import React, { ButtonHTMLAttributes, forwardRef } from 'react'
import { cn } from '@/lib/utils'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'accent' | 'danger'
  size?: 'sm' | 'md' | 'lg'
  fullWidth?: boolean
}

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant = 'primary', size = 'md', fullWidth = false, ...props }, ref) => {
    const baseClasses = 'inline-flex items-center justify-center rounded-md font-medium transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed hover:scale-105 active:scale-95'
    
    const variants = {
      primary: 'bg-primary text-white hover:bg-blue-600 focus:ring-primary',
      secondary: 'bg-gray-500 text-white hover:bg-gray-600 focus:ring-gray-500',
      accent: 'bg-accent text-white hover:bg-orange-600 focus:ring-accent',
      danger: 'bg-red-500 text-white hover:bg-red-600 focus:ring-red-500'
    }
    
    const sizes = {
      sm: 'px-3 py-2 text-small',
      md: 'px-4 py-3 text-body',
      lg: 'px-6 py-4 text-subheading'
    }
    
    const widthClass = fullWidth ? 'w-full' : ''
    
    return (
      <button
        className={cn(
          baseClasses,
          variants[variant],
          sizes[size],
          widthClass,
          className
        )}
        ref={ref}
        {...props}
      />
    )
  }
)

Button.displayName = 'Button'

export { Button }
