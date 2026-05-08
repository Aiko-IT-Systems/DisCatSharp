// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

export default {
  defaultTheme: 'dark',
  disableThemeSelect: true,
  iconLinks: [
    {
      icon: 'github',
      href: 'https://github.com/Aiko-IT-Systems/DisCatSharp',
      title: 'GitHub'
    },
    {
      icon: 'discord',
      href: 'https://discord.gg/RXA6u3jxdU',
      title: 'Discord'
    },
    {
      icon: 'twitter',
      href: 'https://twitter.com/DisCatSharp',
      title: 'Twitter'
    }
  ]
}

document.addEventListener('DOMContentLoaded', () => {
  const header = document.querySelector('.catpunk-header')

  if (header) {
    const setHeaderState = () => header.classList.toggle('is-scrolled', window.scrollY > 12)
    setHeaderState()
    window.addEventListener('scroll', setHeaderState, { passive: true })
  }

  const removeThemeSwitcher = () => {
    const selectors = [
      '[data-bs-theme-value]',
      '[title="changeTheme"]',
      '[aria-label="changeTheme"]',
      '#theme-picker',
      '.theme-picker'
    ]
    document.querySelectorAll(selectors.join(',')).forEach(el => {
      const dropdown = el.closest('.dropdown, .nav-item, li')
      ;(dropdown || el).remove()
    })
  }
  removeThemeSwitcher()
  window.setTimeout(removeThemeSwitcher, 250)
  window.setTimeout(removeThemeSwitcher, 1500)

  const affixPanel = document.querySelector('.catpunk-affix')
  const affix = document.querySelector('#affix')

  if (!affixPanel || !affix) {
    return
  }

  let observer
  let stableTimer

  const updateAffixState = () => {
    const hasList = affix.querySelector('ul') !== null
    const hasItems = affix.querySelector('li') !== null

    affixPanel.classList.toggle('is-empty', !hasList || !hasItems)
  }

  const scheduleStableDisconnect = () => {
    window.clearTimeout(stableTimer)
    stableTimer = window.setTimeout(() => {
      updateAffixState()
      observer?.disconnect()
    }, 1500)
  }

  const runFallbackCheck = () => {
    updateAffixState()
    scheduleStableDisconnect()
  }

  observer = new MutationObserver(() => {
    updateAffixState()
    scheduleStableDisconnect()
  })

  observer.observe(affix, { childList: true, subtree: true })
  updateAffixState()
  scheduleStableDisconnect()

  if ('requestIdleCallback' in window) {
    window.requestIdleCallback(runFallbackCheck, { timeout: 1500 })
  } else {
    window.setTimeout(runFallbackCheck, 500)
  }

  window.setTimeout(runFallbackCheck, 1500)
})
