using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apollo.Database.Migrations;

/// <inheritdoc />
public partial class AddApolloConfigurationTable : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.Sql(@"
      -- Initialize Apollo configuration with default system prompt
      DO $$
      DECLARE
        v_stream_id TEXT := 'apollo_main';
        v_system_prompt TEXT := 'You are a friendly, supportive task assistant who responds in a casual manner. Your name is Apollo, and your identity is that of a large orange male cat. Your main focus is to support neurodivergent people, specifically those with executive function issues, in tracking, maintaining, planning for, and completing tasks. You are a ready listener, but if the user begins to talk about serious subjects that don''t apply to managing tasks, you should encourage them to seek out a friend or a professional to talk to, because you can''t replicate human connection. When setting reminders, ask users for their timezone if they haven''t set one yet, and always confirm reminder times in their local timezone.';
        v_created_on TIMESTAMP := NOW();
      BEGIN
        -- Insert configuration created event
        INSERT INTO mt_events (stream_id, version, data, type, timestamp, mt_dotnet_type)
        VALUES (
          v_stream_id,
          1,
          jsonb_build_object(
            'Key', 'apollo_main',
            'SystemPrompt', v_system_prompt,
            'CreatedOn', v_created_on
          ),
          'configuration_created',
          v_created_on,
          'Apollo.Database.Configuration.Events.ConfigurationCreatedEvent, Apollo.Database'
        );

        -- Create the snapshot in mt_doc_dbapolloconfiguration
        INSERT INTO mt_doc_dbapolloconfiguration (id, data, mt_last_modified, mt_version, mt_dotnet_type)
        VALUES (
          v_stream_id,
          jsonb_build_object(
            'Key', 'apollo_main',
            'SystemPrompt', v_system_prompt,
            'CreatedOn', v_created_on,
            'UpdatedOn', v_created_on
          ),
          v_created_on,
          1,
          'Apollo.Database.Configuration.DbApolloConfiguration, Apollo.Database'
        );
      END $$;
    ");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.Sql(@"
      DELETE FROM mt_doc_dbapolloconfiguration WHERE id = 'apollo_main';
      DELETE FROM mt_events WHERE stream_id = 'apollo_main';
    ");
  }
}
