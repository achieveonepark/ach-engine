import { withMermaid } from 'vitepress-plugin-mermaid'

export default withMermaid({
  base: '/AchEngine/',
  lang: 'ko-KR',
  title: 'AchEngine',
  description: 'VContainer DI · UI System · Addressables · Localization · Table Loader — Unity 통합 툴킷',

  head: [
    ['link', { rel: 'icon', href: '/AchEngine/favicon.svg' }],
    ['meta', { name: 'theme-color', content: '#5d9ecc' }],
  ],

  mermaid: {
    theme: 'base',
    themeVariables: {
      // 기본 색상
      primaryColor: '#1e3a5f',
      primaryTextColor: '#e2e8f0',
      primaryBorderColor: '#3b82f6',
      lineColor: '#64748b',
      secondaryColor: '#0f2d4a',
      tertiaryColor: '#162032',
      // 노드 배경
      background: '#0d1b2a',
      mainBkg: '#1e3a5f',
      nodeBorder: '#3b82f6',
      clusterBkg: '#0f2d4a',
      // 텍스트
      titleColor: '#93c5fd',
      edgeLabelBackground: '#162032',
      // 상태 다이어그램
      stateBkg: '#1e3a5f',
      stateStart: '#3b82f6',
      stateEnd: '#10b981',
      transitionColor: '#64748b',
      // 시퀀스 다이어그램
      actorBkg: '#1e3a5f',
      actorBorder: '#3b82f6',
      actorTextColor: '#e2e8f0',
      actorLineColor: '#64748b',
      signalColor: '#93c5fd',
      signalTextColor: '#e2e8f0',
      activationBkgColor: '#0f2d4a',
      activationBorderColor: '#3b82f6',
      // 폰트
      fontFamily: '"Inter", "Noto Sans KR", sans-serif',
      fontSize: '14px',
    },
  },

  themeConfig: {
    logo: '/logo.svg',
    siteTitle: 'AchEngine',

    nav: [
      { text: '가이드', link: '/guide/' },
      { text: 'GitHub', link: 'https://github.com/achieveonepark/AchEngine', target: '_blank' },
    ],

    sidebar: {
      '/guide/': [
        {
          text: '시작하기',
          items: [
            { text: 'AchEngine이란?', link: '/guide/' },
            { text: '설치', link: '/guide/installation' },
            { text: '빠른 시작', link: '/guide/getting-started' },
          ],
        },
        {
          text: 'DI 시스템',
          items: [
            { text: '개요', link: '/guide/di/' },
            { text: 'AchEngineInstaller', link: '/guide/di/installer' },
            { text: 'ServiceLocator', link: '/guide/di/locator' },
            { text: 'DI 라이프사이클', link: '/guide/di/lifecycle' },
          ],
        },
        {
          text: 'UI 시스템',
          items: [
            { text: '개요', link: '/guide/ui/' },
            { text: 'UIView & 수명 주기', link: '/guide/ui/views' },
            { text: 'UI Workspace', link: '/guide/ui/workspace' },
          ],
        },
        {
          text: 'Table Loader',
          items: [
            { text: '개요', link: '/guide/table/' },
            { text: '설정 & 다운로드', link: '/guide/table/setup' },
            { text: '코드 생성 & 런타임', link: '/guide/table/codegen' },
          ],
        },
        {
          text: 'Addressables',
          items: [
            { text: '개요', link: '/guide/addressables/' },
            { text: '감시 폴더 & 그룹', link: '/guide/addressables/folders' },
            { text: '원격 콘텐츠', link: '/guide/addressables/remote' },
          ],
        },
        {
          text: 'Localization',
          items: [
            { text: '개요', link: '/guide/localization/' },
            { text: '설정 & 데이터베이스', link: '/guide/localization/setup' },
            { text: '키 상수 코드 생성', link: '/guide/localization/codegen' },
          ],
        },
        {
          text: '모듈 연계',
          items: [
            { text: '통합 가이드', link: '/guide/integration' },
          ],
        },
      ],
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/achieveonepark/AchEngine' },
    ],

    footer: {
      message: 'MIT License',
      copyright: 'Copyright © 2024 AchEngine',
    },

    search: {
      provider: 'local',
    },

    editLink: {
      pattern: 'https://github.com/achieveonepark/AchEngine/edit/initial/Docs~/:path',
      text: '이 페이지 수정하기',
    },

    lastUpdated: {
      text: '마지막 업데이트',
    },
  },
})
