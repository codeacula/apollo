import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import SetupWizard from '../../components/SetupWizard.vue'

describe('SetupWizard', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    vi.clearAllMocks()
  })

  /**
   * The setup wizard should render a multi-step form with configuration sections
   * for AI settings (ModelId, Endpoint, ApiKey), Discord settings (Token, PublicKey),
   * and SuperAdmin designation (Discord user ID or OAuth login).
   */
  it('renders all configuration steps', async () => {
    const mockRouterPush = vi.fn().mockResolvedValue(undefined)

    const wrapper = mount(SetupWizard, {
      global: {
        mocks: {
          $router: {
            push: mockRouterPush,
          },
        },
        stubs: {
          teleport: true,
        },
      },
    })

    // Check Step 1 is visible initially with "Step 1 of 3" text
    expect(wrapper.text()).toContain('Step 1 of 3')
    expect(wrapper.text()).toContain('AI Configuration')
    expect(wrapper.find('#modelId').exists()).toBe(true)
    expect(wrapper.find('#endpoint').exists()).toBe(true)
    expect(wrapper.find('#apiKey').exists()).toBe(true)

    // Fill Step 1
    await wrapper.find('#modelId').setValue('gpt-4')
    await wrapper.find('#endpoint').setValue('https://api.openai.com/v1')
    await wrapper.find('#apiKey').setValue('test-key')

    // Click Next to go to Step 2
    const buttons = wrapper.findAll('.btn')
    const nextButton = buttons.find((btn) => btn.text() === 'Next')
    expect(nextButton).toBeDefined()
    await nextButton?.trigger('click')
    await wrapper.vm.$nextTick()

    // Check Step 2 is visible with "Step 2 of 3" text
    expect(wrapper.text()).toContain('Step 2 of 3')
    expect(wrapper.text()).toContain('Discord Configuration')
    expect(wrapper.find('#token').exists()).toBe(true)
    expect(wrapper.find('#publicKey').exists()).toBe(true)
    expect(wrapper.find('#botName').exists()).toBe(true)

    // Fill Step 2
    await wrapper.find('#token').setValue('discord-token')
    await wrapper.find('#publicKey').setValue('public-key')
    await wrapper.find('#botName').setValue('Apollo')

    // Click Next to go to Step 3
    const nextButton2 = wrapper
      .findAll('.btn')
      .find((btn) => btn.text() === 'Next')
    expect(nextButton2).toBeDefined()
    await nextButton2?.trigger('click')
    await wrapper.vm.$nextTick()

    // Check Step 3 is visible with "Step 3 of 3" text
    expect(wrapper.text()).toContain('Step 3 of 3')
    expect(wrapper.text()).toContain('SuperAdmin Designation')
    expect(wrapper.find('#discordUserId').exists()).toBe(true)
  })

  /**
   * When the user completes all steps and clicks submit, the wizard should POST
   * the collected configuration to /api/setup and handle the success response
   * by navigating to the main application view.
   */
  it('submits configuration to API on complete', async () => {
    const mockRouterPush = vi.fn().mockResolvedValue(undefined)
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
    })
    vi.stubGlobal('fetch', mockFetch)

    const wrapper = mount(SetupWizard, {
      global: {
        mocks: {
          $router: {
            push: mockRouterPush,
          },
        },
        stubs: {
          teleport: true,
        },
      },
    })

    // Step 1: Fill AI Configuration
    await wrapper.find('#modelId').setValue('gpt-4')
    await wrapper.find('#endpoint').setValue('https://api.openai.com/v1')
    await wrapper.find('#apiKey').setValue('test-key')
    await wrapper.findAll('.btn').find((btn) => btn.text() === 'Next')?.trigger('click')
    await wrapper.vm.$nextTick()

    // Step 2: Fill Discord Configuration
    await wrapper.find('#token').setValue('discord-token')
    await wrapper.find('#publicKey').setValue('public-key')
    await wrapper.find('#botName').setValue('Apollo')
    await wrapper.findAll('.btn').find((btn) => btn.text() === 'Next')?.trigger('click')
    await wrapper.vm.$nextTick()

    // Step 3: Fill SuperAdmin and Submit
    await wrapper.find('#discordUserId').setValue('123456789')

    // Find and click Submit button
    const submitButton = wrapper
      .findAll('.btn')
      .find((btn) => btn.text().includes('Submit'))
    expect(submitButton).toBeDefined()
    await submitButton?.trigger('click')

    // Wait for async operations to complete
    await flushPromises()
    await wrapper.vm.$nextTick()

    // Assert fetch was called with correct URL and data
    expect(mockFetch).toHaveBeenCalledWith(
      '/api/setup',
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
        }),
      })
    )

    // Verify the request body contains all configuration
    const callArgs = mockFetch.mock.calls[0]
    const body = JSON.parse(callArgs[1].body)
    expect(body.ai.modelId).toBe('gpt-4')
    expect(body.ai.endpoint).toBe('https://api.openai.com/v1')
    expect(body.ai.apiKey).toBe('test-key')
    expect(body.discord.token).toBe('discord-token')
    expect(body.discord.publicKey).toBe('public-key')
    expect(body.discord.botName).toBe('Apollo')
    expect(body.superAdmin.discordUserId).toBe('123456789')

    // Assert router navigation was called
    expect(mockRouterPush).toHaveBeenCalledWith('/dashboard')
  })
})
