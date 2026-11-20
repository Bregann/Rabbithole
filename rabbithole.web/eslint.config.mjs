import js from '@eslint/js'
import tsPlugin from '@typescript-eslint/eslint-plugin'
import tsParser from '@typescript-eslint/parser'
import reactHooks from 'eslint-plugin-react-hooks'

// Rules copied from orbit.web so every project reads the same
const eslintConfig = [
  {
    ignores: [
      'node_modules',
      'dist',
      'eslint.config.mjs',
      'postcss.config.cjs',
    ],
  },
  js.configs.recommended,
  {
    files: ['**/*.ts', '**/*.tsx'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module',
      globals: {
        console: 'readonly',
        process: 'readonly',
        fetch: 'readonly',
        document: 'readonly',
        window: 'readonly',
        URL: 'readonly',
        Response: 'readonly',
        HeadersInit: 'readonly',
        Audio: 'readonly',
        HTMLAudioElement: 'readonly',
        AbortSignal: 'readonly',
        React: 'readonly',
        setTimeout: 'readonly',
        clearTimeout: 'readonly',
      },
      parser: tsParser,
      parserOptions: {
        ecmaFeatures: {
          jsx: true,
        },
        sourceType: 'module',
      },
    },
    plugins: {
      '@typescript-eslint': tsPlugin,
      'react-hooks': reactHooks,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      'no-trailing-spaces': ['error'],
      'object-curly-spacing': ['error', 'always'],
      'space-before-function-paren': ['error', {
        anonymous: 'always',
        named: 'never',
        asyncArrow: 'always'
      }],
      'space-before-blocks': ['error', 'always'],
      'keyword-spacing': ['error', { before: true, after: true }],
      'eol-last': ['error', 'always'],
      'no-multi-spaces': ['error'],
      'key-spacing': ['error', {
        beforeColon: false,
        afterColon: true,
        mode: 'strict'
      }],
      semi: ['error', 'never'],
      quotes: ['error', 'single', { avoidEscape: true }],
      indent: ['error', 2, { SwitchCase: 1 }],
      'block-spacing': ['error', 'always'],
      'no-undef': 'error',
      'no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
    }
  }
]

export default eslintConfig
